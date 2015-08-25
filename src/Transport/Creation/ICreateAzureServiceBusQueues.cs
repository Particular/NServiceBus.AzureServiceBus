namespace NServiceBus.AzureServiceBus
{
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    public interface ICreateAzureServiceBusQueues
    {
        QueueDescription Create(string entityPath, NamespaceManager namespaceManager);
        bool Exists(NamespaceManager namespaceClient, string path);
    }
}