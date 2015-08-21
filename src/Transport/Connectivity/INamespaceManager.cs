namespace NServiceBus.AzureServiceBus
{
    using Microsoft.ServiceBus;

    public interface INamespaceManager
    {
        NamespaceManagerSettings Settings { get; }
    }
}