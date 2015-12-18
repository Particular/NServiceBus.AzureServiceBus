namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Transports;

    public interface IConvertOutgoingMessagesToBrokeredMessages
    {
        BrokeredMessage Convert(OutgoingMessage outgoingMessage, DispatchOptions dispatchOptions, RoutingOptions routingOptions);
        IEnumerable<BrokeredMessage> Convert(IEnumerable<Tuple<OutgoingMessage, DispatchOptions>> outgoingMessages, RoutingOptions routingOptions);
    }
}