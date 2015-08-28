namespace NServiceBus.AzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Transports;

    public interface IConvertBrokeredMessagesToIncomingMessages
    {
        IncomingMessage Convert(BrokeredMessage brokeredMessage);
    }
}