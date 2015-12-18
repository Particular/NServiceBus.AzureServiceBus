namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NServiceBus.Transports;

    public interface IRouteOutgoingMessages
    {
        Task RouteBatch(IEnumerable<Tuple<OutgoingMessage, DispatchOptions>> messages, RoutingOptions routingOptions);
    }
}