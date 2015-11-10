namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
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

                var givenTask = RouteOutBatchesAndLogExceptions(batch, routingOptions);

                tasks.Add(givenTask);
            }

            return Task.WhenAll(tasks.ToArray());
        }

        private async Task RouteOutBatchesAndLogExceptions(IEnumerable<TransportOperation> batch, RoutingOptions routingOptions)
        {
            try
            {
                await routeOutgoingMessages.RouteBatchAsync(batch.Select(x => x.Message), routingOptions).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                // ASB team promissed to fix the issue with MessagingEntityNotFoundException (missing entity path) - verify that
                var message = "Failed to dispatch a batch with the following message IDs: " + string.Join(", ", batch.Select(x => x.Message.MessageId));
                logger.Error(message, exception);
                throw;
            }
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