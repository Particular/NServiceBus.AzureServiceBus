namespace NServiceBus
{
    using System.Threading;
    using Settings;
    using Transport.AzureServiceBus;

    class ForwardingTransportInfrastructure : AzureServiceBusTransportInfrastructure
    {
        ForwardingTopologySectionManager topologySectionManager;
        string bundlePrefix;
        volatile int initializeSignaled;

        public ForwardingTransportInfrastructure(SettingsHolder settings) : base(settings)
        {
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Bundling.NumberOfEntitiesInBundle, 1);
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Bundling.BundlePrefix, "bundle-");
        }

        protected override ITopologySectionManagerInternal CreateTopologySectionManager(string defaultAlias, NamespaceConfigurations namespaces, INamespacePartitioningStrategy partitioning, AddressingLogic addressing)
        {
            var endpointName = Settings.EndpointName();
            var numberOfEntitiesInBundle = Settings.Get<int>(WellKnownConfigurationKeys.Topology.Bundling.NumberOfEntitiesInBundle);
            bundlePrefix = Settings.Get<string>(WellKnownConfigurationKeys.Topology.Bundling.BundlePrefix);

            topologySectionManager = new ForwardingTopologySectionManager(defaultAlias, namespaces, endpointName, numberOfEntitiesInBundle, bundlePrefix, partitioning, addressing);
            // utter hack because we require this at resource creation time
            topologySectionManager.Initialize = async () =>
            {
                if (Interlocked.Exchange(ref initializeSignaled, 1) != 0)
                {
                    return;
                }

                var bundleConfigurations = await NumberOfTopicsInBundleCheck.Run(namespaceManager, namespaceConfigurations, bundlePrefix)
                    .ConfigureAwait(false);

                topologySectionManager.BundleConfigurations = bundleConfigurations;
            };
            return topologySectionManager;
        }

        protected override ICreateAzureServiceBusSubscriptionsInternal CreateSubscriptionCreator()
        {
            return new AzureServiceBusSubscriptionCreatorV6(TopologySettings.SubscriptionSettings, Settings);
        }
    }
}