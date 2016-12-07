namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;
    using Microsoft.ServiceBus.Messaging;

    interface IConvertOutgoingMessagesToBrokeredMessagesInternal
    {
        IEnumerable<BrokeredMessage> Convert(IEnumerable<BatchedOperationInternal> outgoingOperations, RoutingOptionsInternal routingOptions);
    }
}