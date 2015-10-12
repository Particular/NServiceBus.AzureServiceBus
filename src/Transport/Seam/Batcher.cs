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

        internal async Task SendInBatches(IEnumerable<TransportOperation> outgoingMessages, BrokeredMessageReceiveContext context)
        {
            var batches = outgoingMessages.GroupBy(x => new
            {
                Hash = ComputeGroupIdFor(x.DispatchOptions)
            });
            var exceptions = new List<Exception>();

            var sendVia = settings.Get<bool>(WellKnownConfigurationKeys.Connectivity.SendViaReceiveQueue);

            foreach (var batch in batches)
            {
                try
                {
                    var routingOptions = new RoutingOptions
                    {
                        SendVia = sendVia,
                        ViaEntityPath = context?.EntityPath,
                        ViaConnectionString = context?.ConnectionString,
                        DispatchOptions = batch.First().DispatchOptions
                    };
                    await routeOutgoingMessages.RouteBatchAsync(batch.Select(x => x.Message), routingOptions);
                }
                catch (Exception ex)
                {
                    var message = "Failed to dispatch a batch with the following message IDs: " + string.Join(", ", batch.Select(x => x.Message.MessageId));
                    logger.Error(message, ex);

                    exceptions.Add(ex);
                }
            }

            if (exceptions.Any())
            {
                throw new AggregateException(exceptions);
            }

            // How do we handle exceptions here?
            //   TimeoutException
            //   InvalidOperationException
            //   MessagingException
            //   ?? MessagingEntityNotFoundException => core only knows about QueueNotFoundException, what if this is a topic?
            //  if different types of exceptions where raised for several batches, what do we do?
        }

        string ComputeGroupIdFor(DispatchOptions dispatchOptions)
        {
            var sb = new StringBuilder();

            var strategy = dispatchOptions.RoutingStrategy as DirectToTargetDestination;
            if (strategy != null)
            {
                sb.Append($"DirectToTargetDestination-{strategy.Destination}");
            }
            else // ToAllSubscribers
            {
                sb.Append($"ToAllSubscribers;-{(dispatchOptions.RoutingStrategy as ToAllSubscribers).EventType}");
            }

            sb.Append($"--RequiredDispatchConsistency-{dispatchOptions.RequiredDispatchConsistency}");

            return sb.ToString();
        }
    }

  
}