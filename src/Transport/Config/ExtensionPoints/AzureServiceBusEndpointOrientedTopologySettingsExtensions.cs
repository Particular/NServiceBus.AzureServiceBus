namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Configuration.AdvancedExtensibility;
    using Settings;
    using Transport.AzureServiceBus;

    /// <summary>Endpoint-Oriented topology specific settings.</summary>
    public static class AzureServiceBusEndpointOrientedTopologySettingsExtensions
    {
        /// <summary>Register publisher endpoint name for a give event type.</summary>
        public static AzureServiceBusEndpointOrientedTopologySettings RegisterPublisher(this AzureServiceBusEndpointOrientedTopologySettings topologySettings, Type type, string publisherName)
        {
            AddScannerForPublisher(topologySettings.GetSettings(), publisherName, new SingleTypeScanner(type));
            return topologySettings;
        }

        /// <summary>Register publisher endpoint name for a give assembly.</summary>
        public static AzureServiceBusEndpointOrientedTopologySettings RegisterPublisher(this AzureServiceBusEndpointOrientedTopologySettings topologySettings, Assembly assembly, string publisherName)
        {
            AddScannerForPublisher(topologySettings.GetSettings(), publisherName, new AssemblyTypesScanner(assembly));
            return topologySettings;
        }

        static void AddScannerForPublisher(SettingsHolder settings, string publisherName, ITypesScanner scanner)
        {
            if (!settings.TryGet<Dictionary<string, List<ITypesScanner>>>(WellKnownConfigurationKeys.Topology.Publishers, out var map))
            {
                map = new Dictionary<string, List<ITypesScanner>>();
                settings.Set(WellKnownConfigurationKeys.Topology.Publishers, map);
            }

            if (!map.ContainsKey(publisherName))
            {
                map[publisherName] = new List<ITypesScanner>();
            }

            map[publisherName].Add(scanner);
        }
    }
}