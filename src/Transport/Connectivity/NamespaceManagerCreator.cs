namespace NServiceBus.AzureServiceBus
{
    using Microsoft.ServiceBus;
    using Settings;

    class NamespaceManagerCreator : ICreateNamespaceManagers
    {
        ReadOnlySettings settings;

        public NamespaceManagerCreator(ReadOnlySettings settings)
        {
            this.settings = settings;
        }

        public INamespaceManager Create(string namespaceName)
        {
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
            var connectionString = namespacesDefinition.GetConnectionString(namespaceName);

            return new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(connectionString));
        }
    }
}