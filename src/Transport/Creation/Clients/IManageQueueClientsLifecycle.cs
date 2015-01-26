namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;

    public interface IManageQueueClientsLifecycle
    {
        QueueClient Get(Address address);
    }
}