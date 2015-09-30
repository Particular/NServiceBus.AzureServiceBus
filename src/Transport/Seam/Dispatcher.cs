namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using NServiceBus.Logging;
    using NServiceBus.Routing;
    using NServiceBus.Transports;

    class Dispatcher : IDispatchMessages
    {
        readonly IRouteOutgoingMessages routeOutgoingMessages;
        ILog logger = LogManager.GetLogger<Dispatcher>();

        public Dispatcher(IRouteOutgoingMessages routeOutgoingMessages)
        {
            this.routeOutgoingMessages = routeOutgoingMessages;
        }

        public async Task Dispatch(IEnumerable<TransportOperation> outgoingMessages)
        {
            var batches = outgoingMessages.GroupBy(x => new { Hash = ComputeGroupIdFor(x.DispatchOptions) });
            var exceptions = new List<Exception>();

            foreach (var batch in batches)
            {
                try
                {
                    await routeOutgoingMessages.RouteBatchAsync(batch.Select(x => x.Message), batch.First().DispatchOptions);
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