namespace NServiceBus.AzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Transports;

    public interface IConvertOutgoingMessagesToBrokeredMessages
    {
        BrokeredMessage Convert(OutgoingMessage outgoingMessage);
    }
}