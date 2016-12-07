namespace NServiceBus.Transport.AzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;

    interface IConvertBrokeredMessagesToIncomingMessagesInternal
    {
        IncomingMessageDetails Convert(BrokeredMessage brokeredMessage);
    }
}