namespace NServiceBus.AzureServiceBus
{
    using Microsoft.ServiceBus;
    using NServiceBus.Settings;

    class NamespaceManagerCreator : ICreateNamespaceManagers
    {
        private readonly ReadOnlySettings settings;

        public NamespaceManagerCreator(ReadOnlySettings settings)
        {
            this.settings = settings;
        }

        public INamespaceManager Create(string namespaceName)
        {
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);
            var connectionString = namespacesDefinition.GetConnectionString(namespaceName);

            return new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(connectionString));
        }
    }
}