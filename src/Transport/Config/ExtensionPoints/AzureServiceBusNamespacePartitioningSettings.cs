namespace NServiceBus
{
    using AzureServiceBus.Connectivity;
    using Configuration.AdvancedExtensibility;
    using Settings;
    using Transport.AzureServiceBus;

    /// <summary>
    /// Namespace partitioning configuration settings.
    /// </summary>
    public class AzureServiceBusNamespacePartitioningSettings : ExposeSettings
    {
        internal AzureServiceBusNamespacePartitioningSettings(SettingsHolder settings) : base(settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Namespace partitioning strategy to use.
        /// <remarks>
        /// Default is <see cref="SingleNamespacePartitioning" />.
        /// Additional strategies are <see cref="RoundRobinNamespacePartitioning" />,
        /// <see cref="FailOverNamespacePartitioning" />,
        /// </remarks>
        /// </summary>
        public AzureServiceBusNamespacePartitioningSettings UseStrategy<T>() where T : INamespacePartitioningStrategy
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Strategy, typeof(T));

            return this;
        }

        /// <summary>
        /// Namespace partitioning strategy to use.
        /// </summary>
        public AzureServiceBusNamespacePartitioningSettings OverrideBrokerSideSubscriptionFilterFactory<T>(T instance) where T : ICreateBrokerSideSubscriptionFilter
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.BrokerSideSubscriptionFilterFactoryInstance, instance);

            return this;
        }

        /// <summary>
        /// Adds a namespace for partitioning.
        /// </summary>
        public void AddNamespace(string name, string connectionString)
        {
            if (!settings.TryGet<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, out var namespaces))
            {
                namespaces = new NamespaceConfigurations();
                settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, namespaces);
            }

            namespaces.Add(name, connectionString, NamespacePurpose.Partitioning);
        }

        SettingsHolder settings;
    }
}