namespace NServiceBus.AzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;

    public interface ICreateMessagingFactories
    {
        MessagingFactory Create(string address);
    }
}