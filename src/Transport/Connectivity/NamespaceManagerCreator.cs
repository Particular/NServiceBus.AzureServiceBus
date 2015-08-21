namespace NServiceBus.AzureServiceBus
{
    using Microsoft.ServiceBus;

    class NamespaceManagerCreator : ICreateNamespaceManagers
    {
        public INamespaceManager Create(string connectionstring)
        {
            return new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(connectionstring));
        }
    }
}