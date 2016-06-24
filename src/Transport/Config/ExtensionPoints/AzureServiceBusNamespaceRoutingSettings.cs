namespace NServiceBus
{
    using AzureServiceBus;
    using Configuration.AdvanceExtensibility;
    using Settings;

    public class AzureServiceBusNamespaceRoutingSettings : ExposeSettings
    {
        SettingsHolder settings;

        public AzureServiceBusNamespaceRoutingSettings(SettingsHolder settings)
            : base(settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Adds a namespace for routing.
        /// </summary>
        public void AddNamespace(string name, string connectionString)
        {
            NamespaceConfigurations namespaces;
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, out namespaces))
            {
                namespaces = new NamespaceConfigurations();
                settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, namespaces);
            }

            namespaces.Add(name, connectionString, NamespacePurpose.Routing);
        }

    }
}