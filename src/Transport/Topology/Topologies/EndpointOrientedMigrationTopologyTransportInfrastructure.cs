namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using NServiceBus.AzureServiceBus.Connectivity;
    using Settings;

    class EndpointOrientedMigrationTopologyTransportInfrastructure : AzureServiceBusTransportInfrastructure
    {
        public EndpointOrientedMigrationTopologyTransportInfrastructure(SettingsHolder settings) : base(settings)
        {
        }

        protected override ITopologySectionManagerInternal CreateTopologySectionManager(string defaultAlias, NamespaceConfigurations namespaces, INamespacePartitioningStrategy partitioning, AddressingLogic addressing)
        {
            var conventions = Settings.Get<Conventions>();
            var publishersConfiguration = new PublishersConfiguration(conventions, Settings);
            var endpointName = Settings.EndpointName();
            var topicSettings = Settings.GetOrCreate<TopologySettings>().TopicSettings;
            var topicCustomizer = topicSettings.DescriptionCustomizer; 
            var brokerSideSubscriptionFilterFactory = (ICreateBrokerSideSubscriptionFilter)Settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.BrokerSideSubscriptionFilterFactoryInstance);

            topicSettings.DescriptionCustomizer = description =>
            {
                // call customer defined one first
                topicCustomizer(description);

                if (description.Path != EndpointOrientedMigrationTopologySectionManager.MigrationTopicName)
                {
                    return;
                }

                description.RequiresDuplicateDetection = true;
                description.DuplicateDetectionHistoryTimeWindow = TimeSpan.FromSeconds(60);
            };

            return new EndpointOrientedMigrationTopologySectionManager(defaultAlias, namespaces, endpointName, publishersConfiguration, partitioning, addressing, brokerSideSubscriptionFilterFactory);
        }

        protected override ICreateAzureServiceBusSubscriptionsInternal CreateSubscriptionCreator()
        {
            return new EndpointOrientedMigrationTopologySubscriptionCreator(new AzureServiceBusForwardingSubscriptionCreator(TopologySettings.SubscriptionSettings), new AzureServiceBusSubscriptionCreatorV6(TopologySettings.SubscriptionSettings));
        }
    }
}