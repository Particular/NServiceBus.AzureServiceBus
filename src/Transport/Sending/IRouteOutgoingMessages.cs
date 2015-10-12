namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NServiceBus.Transports;

    public interface IRouteOutgoingMessages
    {
        Task RouteAsync(OutgoingMessage message, RoutingOptions routingOptions);

        Task RouteBatchAsync(IEnumerable<OutgoingMessage> messages, RoutingOptions routingOptions);
    }
}