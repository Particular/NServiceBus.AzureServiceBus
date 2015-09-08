namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NServiceBus.Transports;

    public interface IRouteOutgoingMessages
    {
        Task RouteAsync(OutgoingMessage message, DispatchOptions dispatchOptions);

        Task RouteBatchAsync(IEnumerable<OutgoingMessage> messages, DispatchOptions dispatchOptions);
    }
}