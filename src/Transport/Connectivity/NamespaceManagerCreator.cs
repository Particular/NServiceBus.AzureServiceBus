namespace NServiceBus.AzureServiceBus
{
    using Microsoft.ServiceBus;
    using NServiceBus.Settings;

    class NamespaceManagerCreator : ICreateNamespaceManagers
    {
        private readonly ReadOnlySettings _settings;

        public NamespaceManagerCreator(ReadOnlySettings settings)
        {
            _settings = settings;
        }

        public INamespaceManager Create(string namespaceName)
        {
            var namespacesDefinition = _settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);
            var connectionString = namespacesDefinition.GetConnectionString(namespaceName);

            return new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(connectionString));
        }
    }
}