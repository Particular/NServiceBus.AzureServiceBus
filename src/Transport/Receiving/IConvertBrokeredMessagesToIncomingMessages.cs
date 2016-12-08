namespace NServiceBus.Transport.AzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;

    interface IConvertBrokeredMessagesToIncomingMessagesInternal
    {
        IncomingMessageDetailsInternal Convert(BrokeredMessage brokeredMessage);
    }
}