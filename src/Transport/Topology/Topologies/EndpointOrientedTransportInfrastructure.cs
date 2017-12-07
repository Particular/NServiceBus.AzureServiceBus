namespace NServiceBus
{
    using Settings;
    using Transport.AzureServiceBus;

    class EndpointOrientedTransportInfrastructure : AzureServiceBusTransportInfrastructure
    {
        public EndpointOrientedTransportInfrastructure(SettingsHolder settings) : base(settings)
        {
        }

        protected override ITopologySectionManagerInternal CreateTopologySectionManager(string defaultAlias, NamespaceConfigurations @namespaces, INamespacePartitioningStrategy partitioning, AddressingLogic addressing)
        {
            var conventions = Settings.Get<Conventions>();
            var publishersConfiguration = new PublishersConfiguration(conventions, Settings);
            var endpointName = Settings.EndpointName();

            return new EndpointOrientedTopologySectionManager(defaultAlias, namespaces, endpointName, publishersConfiguration, partitioning, addressing);
        }

        protected override ICreateAzureServiceBusSubscriptionsInternal CreateSubscriptionCreator()
        {
            return new AzureServiceBusSubscriptionCreatorV6(TopologySettings.SubscriptionSettings);
        }
    }
}