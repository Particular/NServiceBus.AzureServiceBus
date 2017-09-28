namespace NServiceBus
{
    using System.Threading.Tasks;
    using Settings;
    using Transport.AzureServiceBus;

    class ForwardingTransportInfrastructure : AzureServiceBusTransportInfrastructure
    {
        int numberOfEntitiesInBundle;
        string bundlePrefix;
        ForwardingTopologySectionManager topologySectionManager;

        public ForwardingTransportInfrastructure(SettingsHolder settings) : base(settings)
        {
            numberOfEntitiesInBundle = Settings.Get<int>(WellKnownConfigurationKeys.Topology.Bundling.NumberOfEntitiesInBundle);
            bundlePrefix = Settings.Get<string>(WellKnownConfigurationKeys.Topology.Bundling.BundlePrefix);
        }

        protected override ITopologySectionManagerInternal CreateTopologySectionManager(string defaultAlias, NamespaceConfigurations @namespaces, INamespacePartitioningStrategy partitioning, AddressingLogic addressing)
        {
            var endpointName = SettingsExtensions.EndpointName(Settings);
            topologySectionManager = new ForwardingTopologySectionManager(defaultAlias, @namespaces, endpointName, numberOfEntitiesInBundle, bundlePrefix, partitioning, addressing);
            return topologySectionManager;
        }

        protected override ICreateAzureServiceBusSubscriptionsInternal CreateSubscriptionCreator()
        {
            return new AzureServiceBusSubscriptionCreatorV6(TopologySettings.SubscriptionSettings, Settings);
        }

        public override async Task Start()
        {
            await base.Start().ConfigureAwait(false);

            var bundleConfigurations = await NumberOfTopicsInBundleCheck.Run(namespaceManager, namespaceConfigurations, bundlePrefix)
                .ConfigureAwait(false);
            topologySectionManager.BundleConfigurations = bundleConfigurations;
        }
    }
}