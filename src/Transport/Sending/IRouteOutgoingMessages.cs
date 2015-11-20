namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NServiceBus.Transports;

    public interface IRouteOutgoingMessages
    {
        Task RouteBatch(IEnumerable<OutgoingMessage> messages, RoutingOptions routingOptions);
    }
}