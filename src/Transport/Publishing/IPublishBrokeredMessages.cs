namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;

    public interface IPublishBrokeredMessages
    {
        void Publish(BrokeredMessage brokeredMessage);
    }
}