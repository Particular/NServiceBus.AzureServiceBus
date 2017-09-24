namespace NServiceBus
{
    using Configuration.AdvancedExtensibility;
    using Settings;
    using Transport.AzureServiceBus;

    public class AzureServiceBusNamespaceRoutingSettings : ExposeSettings
    {
        internal AzureServiceBusNamespaceRoutingSettings(SettingsHolder settings) : base(settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Adds a namespace for routing.
        /// </summary>
        public NamespaceInfo AddNamespace(string name, string connectionString)
        {
            if (!settings.TryGet<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, out var namespaces))
            {
                namespaces = new NamespaceConfigurations();
                settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, namespaces);
            }

            return namespaces.Add(name, connectionString, NamespacePurpose.Routing);
        }

        SettingsHolder settings;
    }
}