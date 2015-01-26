namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;

    public interface ICreateMessagingFactories
    {
        MessagingFactory Create(Address address);
    }
}