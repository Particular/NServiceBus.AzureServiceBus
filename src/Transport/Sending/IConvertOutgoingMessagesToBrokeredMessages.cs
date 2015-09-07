namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Transports;

    public interface IConvertOutgoingMessagesToBrokeredMessages
    {
        BrokeredMessage Convert(OutgoingMessage outgoingMessage, DispatchOptions dispatchOptions);
        IEnumerable<BrokeredMessage> Convert(IEnumerable<OutgoingMessage> outgoingMessages, DispatchOptions dispatchOptions);
    }
}