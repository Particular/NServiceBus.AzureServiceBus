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
            NamespaceConfigurations namespaces;
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, out namespaces))
            {
                namespaces = new NamespaceConfigurations();
                settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, namespaces);
            }

            return namespaces.Add(name, connectionString, NamespacePurpose.Routing);
        }

        SettingsHolder settings;
    }
}