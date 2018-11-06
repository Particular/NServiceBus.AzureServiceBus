namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
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
            var topicSettings = Settings.GetOrCreate<TopologySettings>().TopicSettings;
            var customizer = topicSettings.DescriptionCustomizer;
            topicSettings.DescriptionCustomizer = description =>
            {
                // call customer defined one first
                customizer(description);

                if (description.Path != EndpointOrientedTopologyMigrationSectionManager.MigrationTopicName)
                {
                    return;
                }

                description.RequiresDuplicateDetection = true;
                description.DuplicateDetectionHistoryTimeWindow = TimeSpan.FromSeconds(60);
            };

            return new EndpointOrientedTopologyMigrationSectionManager(defaultAlias, namespaces, endpointName, publishersConfiguration, partitioning, addressing);
        }

        protected override ICreateAzureServiceBusSubscriptionsInternal CreateSubscriptionCreator()
        {
            return new EndpointOrientedTopologyMigrationSubscriptionCreator(new AzureServiceBusForwardingSubscriptionCreator(TopologySettings.SubscriptionSettings), new AzureServiceBusSubscriptionCreatorV6(TopologySettings.SubscriptionSettings));
        }
    }
}