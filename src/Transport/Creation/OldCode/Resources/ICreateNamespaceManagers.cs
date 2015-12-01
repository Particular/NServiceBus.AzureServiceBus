namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using Microsoft.ServiceBus;

    public interface ICreateNamespaceManagers
    {
        NamespaceManager Create(string serviceBusNamespace);
    }
}