namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using Microsoft.ServiceBus.Messaging;

    public interface IConvertOutgoingMessagesToBrokeredMessages
    {
        BrokeredMessage Convert(BatchedOperation outgoingOperation, RoutingOptions routingOptions);
        IEnumerable<BrokeredMessage> Convert(IEnumerable<BatchedOperation> outgoingOperations, RoutingOptions routingOptions);
    }
}