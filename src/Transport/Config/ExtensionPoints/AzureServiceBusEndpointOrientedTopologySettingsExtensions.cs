namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using Configuration.AdvancedExtensibility;
    using Settings;
    using Transport.AzureServiceBus;

    public static partial class AzureServiceBusEndpointOrientedTopologySettingsExtensions
    {
        public static AzureServiceBusEndpointOrientedTopologySettings RegisterPublisher(this AzureServiceBusEndpointOrientedTopologySettings topologySettings, Type type, string publisherName)
        {
            AddScannerForPublisher(topologySettings.GetSettings(), publisherName, new SingleTypeScanner(type));
            return topologySettings;
        }

        public static AzureServiceBusEndpointOrientedTopologySettings RegisterPublisher(this AzureServiceBusEndpointOrientedTopologySettings topologySettings, Assembly assembly, string publisherName)
        {
            AddScannerForPublisher(topologySettings.GetSettings(), publisherName, new AssemblyTypesScanner(assembly));
            return topologySettings;
        }

        static void AddScannerForPublisher(SettingsHolder settings, string publisherName, ITypesScanner scanner)
        {
            Dictionary<string, List<ITypesScanner>> map;

            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Publishers, out map))
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