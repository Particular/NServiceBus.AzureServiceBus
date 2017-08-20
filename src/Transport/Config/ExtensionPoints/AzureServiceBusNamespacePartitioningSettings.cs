namespace NServiceBus
{
    using Configuration.AdvanceExtensibility;
    using Settings;
    using Transport.AzureServiceBus;

    public partial class AzureServiceBusNamespacePartitioningSettings : ExposeSettings
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
        public AzureServiceBusNamespacePartitioningSettingsExtensionPoint<T> UseStrategy<T>(T strategy) where T : INamespacePartitioningStrategy
        {
            return new AzureServiceBusNamespacePartitioningSettingsExtensionPoint<T>(settings, strategy);
        }

        /// <summary>
        /// Adds a namespace for partitioning.
        /// </summary>
        public void AddNamespace(string name, string connectionString)
        {
            NamespaceConfigurations namespaces;
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, out namespaces))
            {
                namespaces = new NamespaceConfigurations();
                settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, namespaces);
            }

            namespaces.Add(name, connectionString, NamespacePurpose.Partitioning);
        }

        SettingsHolder settings;
    }
}