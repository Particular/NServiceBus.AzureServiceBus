namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading.Tasks;
    using NServiceBus.Logging;
    using NServiceBus.Routing;
    using NServiceBus.Transports;

    class Dispatcher : IDispatchMessages
    {
        readonly IRouteOutgoingMessages routeOutgoingMessages;
        ILog log = LogManager.GetLogger<Dispatcher>();

        public Dispatcher(IRouteOutgoingMessages routeOutgoingMessages)
        {
            this.routeOutgoingMessages = routeOutgoingMessages;
        }

        public Task Dispatch(IEnumerable<TransportOperation> outgoingMessages)
        {
            // messages should be grouped by the following hash
            // + identical routing strategy and it's value, queue ((DirectToTargetDestination => Destination) or event type (ToAllSubscribers => EventType)
            // + identical dispatch consistency
            // - delivery constraints doesn't matter
            // - contextbag doesn't matter

            var groups = outgoingMessages.GroupBy(x => new { Hash = ComputeHashFor(x.DispatchOptions)});

            foreach (var @group in groups)
            {
                try
                {
                    routeOutgoingMessages.RouteBatchAsync(@group.Select(x => x.Message), @group.FirstOrDefault().DispatchOptions);
                }
                catch (Exception exception)
                {
                    // TODO: 
                    // Should we spawn off multiple tasks and perform Task.WhenAll(t1, t2, ... tn)?
                    // Should we have a continuation with Failed condition to signal to core that we've failed?
                    // How do we handle exceptions here?
                    //   TimeoutException
                    //   InvalidOperationException
                    //   MessagingException
                    log.Error("Failed to dispatch messages.", exception);
                }
            }

            return TaskEx.Completed;
        }

        string ComputeHashFor(DispatchOptions dispatchOptions)
        {
            var sb = new StringBuilder();

            var cryptoServiceProvider = new MD5CryptoServiceProvider();

            var strategy = dispatchOptions.RoutingStrategy as DirectToTargetDestination;
            if (strategy != null)
            {
                sb.Append($"DirectToTargetDestination-{strategy.Destination}");
            }
            else // ToAllSubscribers
            {
                sb.Append($"ToAllSubscribers;-{(dispatchOptions.RoutingStrategy as ToAllSubscribers).EventType}");
            }

            sb.Append($"RequiredDispatchConsistency-{dispatchOptions.RequiredDispatchConsistency}");

            var bytes = Encoding.Default.GetBytes(sb.ToString());
            var computeHash = cryptoServiceProvider.ComputeHash(bytes);

            var result = BitConverter.ToString(computeHash);
            return result;
        }
    }
}