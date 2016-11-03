namespace NServiceBus
{
    using Configuration.AdvanceExtensibility;
    using Settings;
    using Transport.AzureServiceBus;

    public class AzureServiceBusNamespacePartitioningSettings : ExposeSettings
    {
        SettingsHolder settings;

        [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public AzureServiceBusNamespacePartitioningSettings(SettingsHolder settings) : base(settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Namespace partitioning strategy to use.
        /// <remarks> Default is <see cref="SingleNamespacePartitioning"/>. 
        /// Additional strategies are <see cref="RoundRobinNamespacePartitioning"/>,
        /// <see cref="FailOverNamespacePartitioning"/>,
        /// </remarks>
        /// </summary>
        [ObsoleteEx(RemoveInVersion = "9.0", TreatAsErrorFromVersion = "8.0", ReplacementTypeOrMember = "With<T>() where T : IPartitioningStrategy")]
        public AzureServiceBusNamespacePartitioningSettings UseStrategy<T>() where T : INamespacePartitioningStrategy
        {
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Strategy, typeof(T));

            return this;
        }

        /// <summary>
        /// Namespace partitioning strategy to use.
        /// <remarks> Default is <see cref="SinglePartitioning"/>. 
        /// Additional strategies are <see cref="RoundRobinPartitioning"/>,
        /// <see cref="FailOverPartitioning"/>,
        /// </remarks>
        /// </summary>
        public AzureServiceBusNamespacePartitioningSettings With<T>() where T : IPartitioningStrategy, new()
        {
            settings.Set<IPartitioningStrategy>(new T());

            return this;
        }

        /// <summary>
        /// Namespace partitioning strategy to use.
        /// <remarks> Default is <see cref="SinglePartitioning"/>. 
        /// Additional strategies are <see cref="RoundRobinNamespacePartitioning"/>,
        /// <see cref="FailOverPartitioning"/>,
        /// </remarks>
        /// </summary>
        public AzureServiceBusNamespacePartitioningSettings With(IPartitioningStrategy strategy)
        {
            settings.Set<IPartitioningStrategy>(strategy);

            return this;
        }

        /// <summary>
        /// Adds a namespace for partitioning.
        /// </summary>
        public void AddNamespace( string name, string connectionString)
        {
            NamespaceConfigurations namespaces;
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, out namespaces))
            {
                namespaces = new NamespaceConfigurations();
                settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, namespaces);
            }

            namespaces.Add(name, connectionString, NamespacePurpose.Partitioning);
        }

    }
}