namespace NServiceBus.AzureServiceBus.Addressing.Partitioning
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using Settings;
    using Transport.AzureServiceBus;

    public static class ReadOnlySettingsExtensions
    {
        public static List<RuntimeNamespaceInfo> GetPartitioningNamespaces(this ReadOnlySettings settings)
        {
            NamespaceConfigurations namespaces;
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, out namespaces))
            {
                throw new ConfigurationErrorsException($"No namespaces have been defined, please configure the connection string to your azure servicebus namespace. Please use {nameof(AzureServiceBusTransportExtensions.NamespacePartitioning)}().{nameof(AzureServiceBusNamespacePartitioningSettings.AddNamespace)}() to register one or more namespaces.");
            }

            return namespaces.Where(n => n.Purpose == NamespacePurpose.Partitioning).Select(n => new RuntimeNamespaceInfo(n)).ToList();
        }
    }
}
