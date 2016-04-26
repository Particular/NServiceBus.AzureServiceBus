namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using Microsoft.ServiceBus.Messaging;

    public interface IConvertOutgoingMessagesToBrokeredMessages
    {
        IEnumerable<BrokeredMessage> Convert(IEnumerable<BatchedOperation> outgoingOperations, RoutingOptions routingOptions);
    }
}