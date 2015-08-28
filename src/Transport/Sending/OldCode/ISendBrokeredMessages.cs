namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;

    public interface ISendBrokeredMessages
    {
        void Send(BrokeredMessage brokeredMessage);
    }
}