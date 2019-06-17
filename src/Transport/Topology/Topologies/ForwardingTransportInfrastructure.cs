namespace NServiceBus
{
    using System.Threading;
    using AzureServiceBus.Connectivity;
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
            var brokerSideSubscriptionFilterFactory = (ICreateBrokerSideSubscriptionFilter)Settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.BrokerSideSubscriptionFilterFactoryInstance);

            topologySectionManager = new ForwardingTopologySectionManager(defaultAlias, namespaces, endpointName, numberOfEntitiesInBundle, bundlePrefix, partitioning, addressing, brokerSideSubscriptionFilterFactory);
            // By design the topology section manager should determine the resources to create without needing information
            // from ASB. When we realized one bundle was enough we had to call out for backward compatibility reasons to query
            // how many bundles there are. This is async but should happen outside the actual section manager. Thus
            // the callback was introduced.
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
            return new AzureServiceBusForwardingSubscriptionCreator(TopologySettings.SubscriptionSettings);
        }
    }
}