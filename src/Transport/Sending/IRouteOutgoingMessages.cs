namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NServiceBus.Transports;

    public interface IRouteOutgoingMessages
    {
        Task RouteBatchAsync(IEnumerable<OutgoingMessage> messages, RoutingOptions routingOptions);
    }
}