// just sample code, make this work

//using NServiceBus;
//using NServiceBus.Azure.Transports.WindowsAzureServiceBus;

//namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
//{
//    using System;
//    using System.Collections.Concurrent;
//    using System.Collections.Generic;
//    using System.Linq;
//    using System.Runtime.CompilerServices;
//    using NServiceBus.DelayedDelivery;
//    using NServiceBus.Extensibility;
//    using NServiceBus.Pipeline;
//    using NServiceBus.Pipeline.Contexts;
//    using NServiceBus.Routing;
//    using NServiceBus.Transports;

//    class AzureServiceBusDispatcher : IDispatchMessages //ISendMessages, IDeferMessages
//    {
//        ITopologySectionManager topology;
//        ConditionalWeakTable<IncomingMessage, ConcurrentDictionary<string, List<OutgoingMessage>>> batchTrackingContext;

//        public AzureServiceBusDispatcher(ITopologySectionManager topology, Configure config)
//        {
//            this.topology = topology;
//            batchTrackingContext = new ConditionalWeakTable<IncomingMessage, ConcurrentDictionary<string, List<OutgoingMessage>>>();
//        }

//        public void Dispatch(OutgoingMessage message, DispatchOptions dispatchOptions)
//        {
//            var deliveryConstraint = dispatchOptions.DeliveryConstraints.FirstOrDefault(d => d is DelayedDeliveryConstraint);

//            if (deliveryConstraint != null)
//            {
//                var deliveryConstraintWithTimespan = deliveryConstraint as DelayDeliveryWith;
//                if (deliveryConstraintWithTimespan != null)
//                {
                    
//                }
//                else // DoNotDeliverBefore case
//                {
//                    var delayDeliveryWithExactTime = deliveryConstraint as DoNotDeliverBefore;
//                }
//            }

//            // TODO: check for TTBR (constraint name: DiscardIfNotReceivedBefore)

//            var directRouting = dispatchOptions.RoutingStrategy as DirectToTargetDestination; // string destination on this

//            if (directRouting == null) // publish
//            {
//                var toAllSubscribers = dispatchOptions.RoutingStrategy as ToAllSubscribers;
//            }
//            else // send
//            {
//            }

//            var batch = dispatchOptions.Context.Get<AzureServiceBusBatchingBehavior.AzureServiceBusBatchMetadata>();
//            if (batch != null)
//            {
//                // first, add outgoing message to the in-memory collection
//                var outgoingContext = (OutgoingContext)dispatchOptions.Context;
//                TransportMessage incomingMessage;
//                outgoingContext.TryGetIncomingPhysicalMessage(out incomingMessage);



//                if (batch.Commit)
//                {
//                    // dispatch/send and array of messages (native ASB)
//                    // do not continue with OutgoingMessage that kicked off this dispatch call
//                }
                
//            }
//            else
//            {
//                // send single message (native ASB) - dispatch message as there's no batching going on for it
//            }
//        }
//    }


//    class AzureServiceBusBatchingBehavior : Behavior<OutgoingContext>
//    {
//        public class AzureServiceBusBatchMetadata
//        {
//            public string BatchId { get; private set; }
//            public bool Commit { get; set; }

//            // TODO: User provides BatchIdGenerator, and if not present, we fallback to GUID
//            public AzureServiceBusBatchMetadata(string batchId)
//            {
//                if (string.IsNullOrWhiteSpace(batchId))
//                {
//                    batchId = Guid.NewGuid().ToString();
//                }
//                BatchId = batchId;
//            }
//        }

//        public override void Invoke(OutgoingContext context, Action next)
//        {
//            AzureServiceBusBatchMetadata azureServiceBusBatchMetadata;
//            if (context.Extensions.TryGet(out azureServiceBusBatchMetadata))
//            {
//                context.Set(azureServiceBusBatchMetadata);
//            }
//            next();
//        }
//    }

//    public static class AzureServiceBusOptionsExtensions
//    {
//        public static void Batch(this SendOptions options, string batchId)
//        {
//            AzureServiceBusBatchingBehavior.AzureServiceBusBatchMetadata batchMetadata;
//            if (!options.GetExtensions().TryGet(out batchMetadata))
//            {
//                options.GetExtensions().Set(new AzureServiceBusBatchingBehavior.AzureServiceBusBatchMetadata(batchId));
//            }

//            if (batchId != batchMetadata.BatchId)
//            {
//                throw new InvalidOperationException(string.Format("A previous batch is used (batch ID='{0}'). To start a new batch, create a new SendOptions object specifying a new batch ID.", batchMetadata.BatchId));
//            }
//        }

//        public static void Commit(this SendOptions options)
//        {
//            AzureServiceBusBatchingBehavior.AzureServiceBusBatchMetadata batchMetadata;
//            if (!options.GetExtensions().TryGet(out batchMetadata))
//            {
//                throw new Exception("No sending batch was started. Use sendOptions.Batch(id) to start one before using sendOptions.Commit()");
//            }
//            batchMetadata.Commit = true;
//        }
//    }
//}


//class how_would_a_user_use_batching
//{
//    public static void sending_4_messages_in_2_batches()
//    {
//        var sendOptions = new SendOptions();
//        sendOptions.Batch("batch-1");
//        dynamic bus = new object();
//        bus.Send(new {}, sendOptions); // cmd 1
//        bus.Send(new { }, sendOptions); // cmd 2

//        sendOptions = new SendOptions();
//        sendOptions.Batch("batch-2");
//        bus.Send(new { }, sendOptions); // cmd 3
//        bus.Send(new { }, sendOptions); // cmd 4

//        sendOptions.Commit(); // will need a fake send to trigger message dispatching
//        bus.Send(new
//        {
//            desc = "fake message"
//        });

//        //eventually
//        bus.Send(new []{ new {}, new {}}, "batch-1");
//        bus.Send(new []{ new {}, new {}}, "batch-2");
//        bus.Send(new {}, "batch-2");
//        bus.Commit();
//    }
//}