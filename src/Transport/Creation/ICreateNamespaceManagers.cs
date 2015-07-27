namespace NServiceBus.AzureServiceBus
{
    using Microsoft.ServiceBus;

    public interface ICreateNamespaceManagers
    {
        NamespaceManager Create(string connectionstring);
    }
}