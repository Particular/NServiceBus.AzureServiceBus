namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;
    using Microsoft.ServiceBus.Messaging;

    [ObsoleteEx(Message = "Internal contract that shouldn't be exposed.", TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IConvertOutgoingMessagesToBrokeredMessages
    {
        IEnumerable<BrokeredMessage> Convert(IEnumerable<BatchedOperation> outgoingOperations, RoutingOptions routingOptions);
    }
}