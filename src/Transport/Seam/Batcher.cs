namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Logging;
    using NServiceBus.Routing;
    using NServiceBus.Settings;
    using NServiceBus.Transports;

    class Batcher
    {
        ILog logger = LogManager.GetLogger<Batcher>();
        readonly IRouteOutgoingMessages routeOutgoingMessages;
        readonly ReadOnlySettings settings;

        public Batcher(IRouteOutgoingMessages routeOutgoingMessages, ReadOnlySettings settings)
        {
            this.routeOutgoingMessages = routeOutgoingMessages;
            this.settings = settings;
        }

        internal Task SendInBatches(IEnumerable<TransportOperation> outgoingMessages, BrokeredMessageReceiveContext context)
        {
            var batches = outgoingMessages.GroupBy(x => new
            {
                Hash = ComputeGroupIdFor(x.DispatchOptions)
            });
            var tasks = new List<Task>();

            var sendVia = settings.Get<bool>(WellKnownConfigurationKeys.Connectivity.SendViaReceiveQueue);

            foreach (var batch in batches)
            {
                var routingOptions = new RoutingOptions
                {
                    SendVia = sendVia,
                    ViaEntityPath = context?.EntityPath,
                    ViaConnectionString = context?.ConnectionString,
                    DispatchOptions = batch.First().DispatchOptions
                };

                var givenTask = routeOutgoingMessages.RouteBatchAsync(batch.Select(x => x.Message), routingOptions);
                givenTask.ContinueWith(task =>
                {
                    task.Exception?.Handle(exception =>
                    {
                        if (exception is MessagingEntityNotFoundException)
                        {
                            // Sending Via
                            if (routingOptions.ViaEntityPath != null)
                            {
                                logger.Error($"Entity '{routingOptions.SendVia}' does not exist.");
                            }
                            else // immediately sent case
                            {
                                var commandStrategy = routingOptions.DispatchOptions.AddressTag as UnicastAddressTag;
                                if (commandStrategy != null)
                                {
                                    logger.Error($"Entity '{commandStrategy.Destination}' does not exist.");
                                }
                                else // MulticastAddressTag
                                {
                                    // TODO: event destination entity depends on the topology used. What do we log here to help users?
                                    //(routingOptions.DispatchOptions.AddressTag as MulticastAddressTag).MessageType
                                }
                            }
                        }

                        var message = "Failed to dispatch a batch with the following message IDs: " + string.Join(", ", batch.Select(x => x.Message.MessageId));
                        logger.Error(message, exception);

                        return false;
                    });
                });

                tasks.Add(givenTask);
            }

            return Task.WhenAll(tasks.ToArray());
        }

        string ComputeGroupIdFor(DispatchOptions dispatchOptions)
        {
            var sb = new StringBuilder();

            var strategy = dispatchOptions.AddressTag as UnicastAddressTag;
            if (strategy != null)
            {
                sb.Append($"DirectToTargetDestination-{strategy.Destination}");
            }
            else // multicast
            {
                sb.Append($"ToAllSubscribers;-{(dispatchOptions.AddressTag as MulticastAddressTag).MessageType}");
            }

            sb.Append($"--RequiredDispatchConsistency-{dispatchOptions.RequiredDispatchConsistency}");

            return sb.ToString();
        }
    }

  
}