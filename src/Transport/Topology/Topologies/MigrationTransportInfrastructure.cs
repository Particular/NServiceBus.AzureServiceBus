namespace NServiceBus
{
    using Settings;
    using Transport.AzureServiceBus;

    class MigrationTransportInfrastructure : AzureServiceBusTransportInfrastructure
    {
        public MigrationTransportInfrastructure(SettingsHolder settings) : base(settings)
        {
        }

        protected override ITopologySectionManagerInternal CreateTopologySectionManager(string defaultAlias, NamespaceConfigurations @namespaces, INamespacePartitioningStrategy partitioning, AddressingLogic addressing)
        {
            var conventions = Settings.Get<Conventions>();
            var publishersConfiguration = new PublishersConfiguration(conventions, Settings);
            var endpointName = Settings.EndpointName();

            return new MigrationTopologySectionManager(defaultAlias, namespaces, endpointName, publishersConfiguration, partitioning, addressing);
        }

        protected override ICreateAzureServiceBusSubscriptionsInternal CreateSubscriptionCreator()
        {
            return new MigrationSubscriptionCreator(new AzureServiceBusForwardingSubscriptionCreator(TopologySettings.SubscriptionSettings), new AzureServiceBusSubscriptionCreatorV6(TopologySettings.SubscriptionSettings));
        }
    }
}