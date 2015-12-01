namespace NServiceBus.AzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;
    
    public interface IConvertBrokeredMessagesToIncomingMessages
    {
        IncomingMessageDetails Convert(BrokeredMessage brokeredMessage);
    }
}