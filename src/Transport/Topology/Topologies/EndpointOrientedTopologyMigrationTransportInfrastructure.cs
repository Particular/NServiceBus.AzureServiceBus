namespace NServiceBus.Transport.AzureServiceBus
{
    using Settings;

    class EndpointOrientedTopologyMigrationTransportInfrastructure : AzureServiceBusTransportInfrastructure
    {
        public EndpointOrientedTopologyMigrationTransportInfrastructure(SettingsHolder settings) : base(settings)
        {
        }

        protected override ITopologySectionManagerInternal CreateTopologySectionManager(string defaultAlias, NamespaceConfigurations @namespaces, INamespacePartitioningStrategy partitioning, AddressingLogic addressing)
        {
            var conventions = Settings.Get<Conventions>();
            var publishersConfiguration = new PublishersConfiguration(conventions, Settings);
            var endpointName = Settings.EndpointName();

            return new EndpointOrientedTopologyMigrationSectionManager(defaultAlias, namespaces, endpointName, publishersConfiguration, partitioning, addressing);
        }

        protected override ICreateAzureServiceBusSubscriptionsInternal CreateSubscriptionCreator()
        {
            return new EndpointOrientedTopologyMigrationSubscriptionCreator(new AzureServiceBusForwardingSubscriptionCreator(TopologySettings.SubscriptionSettings), new AzureServiceBusSubscriptionCreatorV6(TopologySettings.SubscriptionSettings));
        }
    }
}