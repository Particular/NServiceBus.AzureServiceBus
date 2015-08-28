namespace NServiceBus.AzureServiceBus
{
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    public interface ICreateAzureServiceBusQueues
    {
        QueueDescription Create(string queuePath, NamespaceManager namespaceManager);
    }
}