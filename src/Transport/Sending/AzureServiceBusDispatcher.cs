namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using System;
    using System.Linq;
    using System.Transactions;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.DelayedDelivery;
    using NServiceBus.DeliveryConstraints;
    using NServiceBus.Extensibility;
    using NServiceBus.Pipeline;
    using NServiceBus.Pipeline.Contexts;
    using NServiceBus.Routing;
    using NServiceBus.Transports;
    using Unicast;
    using Unicast.Queuing;
    using SendOptions = NServiceBus.SendOptions;

    class AzureServiceBusDispatcher : IDispatchMessages //ISendMessages, IDeferMessages
    {
        ITopology topology;
        Configure config;

        public AzureServiceBusDispatcher(ITopology topology, Configure config)
        {
            this.topology = topology;
            this.config = config;
        }

        public void Send(TransportMessage message, SendOptions options)
        {
            SendInternal(message, options);
        }

        public void Defer(TransportMessage message, SendOptions options)
        {
            SendInternal(message, options, expectDelay: true);
        }

        public void ClearDeferredMessages(string headerKey, string headerValue)
        {
            //? throw new NotSupportedException();
        }

        void SendInternal(TransportMessage message, SendOptions options, bool expectDelay = false)
        {
            var sender = topology.GetSender(options.Destination);
       
            if (!config.Settings.Get<bool>("Transactions.Enabled") || Transaction.Current == null)
            {
                SendInternal(message, sender, options, expectDelay);
            }
            else
            {
                Transaction.Current.EnlistVolatile(new SendResourceManager(() => SendInternal(message, sender, options, expectDelay)), EnlistmentOptions.None);
            }
        }

        void SendInternal(TransportMessage message, ISendBrokeredMessages sender, SendOptions options, bool expectDelay)
        {
            try
            {
                using(var brokeredMessage = message.ToBrokeredMessage(options, config.Settings, expectDelay, config))
                {
                    if (brokeredMessage != null)
                    {
                        sender.Send(brokeredMessage);
                    }
                }
            }
            catch (MessagingEntityNotFoundException)
            {
                throw new QueueNotFoundException
                {
                    Queue = options.Destination
                };
            }
        }

        public void Dispatch(OutgoingMessage message, DispatchOptions dispatchOptions)
        {
            var deliveryConstraint = dispatchOptions.DeliveryConstraints.FirstOrDefault(d => d is DelayedDeliveryConstraint);

            if (deliveryConstraint != null)
            {
                var deliveryConstraintWithTimespan = deliveryConstraint as DelayDeliveryWith;
                if (deliveryConstraintWithTimespan != null)
                {
                    
                }
                else // DoNotDeliverBefore case
                {
                    var delayDeliveryWithExactTime = deliveryConstraint as DoNotDeliverBefore;
                }
            }

            // TODO: check for TTBR (constraint name: DiscardIfNotReceivedBefore)

            var directRouting = dispatchOptions.RoutingStrategy as DirectToTargetDestination; // string destination on this

            if (directRouting == null) // publish
            {
                var toAllSubscribers = dispatchOptions.RoutingStrategy as ToAllSubscribers;
            }
            else // send
            {
            }

            var batch = dispatchOptions.Context.Get<AzureServiceBusBatchingBehavior.AzureServiceBusBatch>();
            if (batch != null)
            {
                // first, add outgoing message to the in-memory collection

                if (batch.Commit)
                {
                    // dispatch/send and array of messages (native ASB)
                }
                
            }
            else
            {
                // send single message (native ASB)
            }
        }
    }


    class AzureServiceBusBatchingBehavior : Behavior<OutgoingContext>
    {
        public class AzureServiceBusBatch
        {
            public bool Commit { get; set; }
        }

        public override void Invoke(OutgoingContext context, Action next)
        {
            AzureServiceBusBatch azureServiceBusBatch;
            if (context.Extensions.TryGet(out azureServiceBusBatch))
            {
                context.Set(azureServiceBusBatch);
            }
            next();
        }
    }

    public static class AzureServiceBusOptionExtension
    {
        public static void Batch(this SendOptions options)
        {
            AzureServiceBusBatchingBehavior.AzureServiceBusBatch batch;
            if (!options.GetExtensions().TryGet(out batch))
            {
                options.GetExtensions().Set(new AzureServiceBusBatchingBehavior.AzureServiceBusBatch());
            }
        }

        public static void Commit(this SendOptions options)
        {
            AzureServiceBusBatchingBehavior.AzureServiceBusBatch batch;
            if (!options.GetExtensions().TryGet(out batch))
            {
                throw new Exception("No sending batch was started. Use sendOptions.Batch() to start one before using sendOptions.Commit()");
            }
            batch.Commit = true;
        }
    }
}