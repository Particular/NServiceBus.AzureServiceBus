namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.AzureServiceBus.EventsScanner;
    using NServiceBus.Settings;

    public class AzureServiceBusTopologySettings : TransportExtensions<AzureServiceBusTransport>
    {
        private readonly SettingsHolder _settings;

        public AzureServiceBusTopologySettings(SettingsHolder settings)
            : base(settings)
        {
            _settings = settings;
        }

        public AzureServiceBusTopologySettings RegisterPublisherForType(string publisherName, Type type)
        {
            AddScannerForPublisher(publisherName, new TypeEventsScanner(type));
            return this;
        }

        public AzureServiceBusTopologySettings RegisterPublisherForAssembly(string publisherName, string assemblyName)
        {
            AddScannerForPublisher(publisherName, new AssemblyEventsScanner(assemblyName));
            return this;
        }

        private void AddScannerForPublisher(string publisherName, IEventsScanner scanner)
        {
            Dictionary<string, List<IEventsScanner>> map;

            if (!_settings.TryGet(WellKnownConfigurationKeys.Topology.Publishers, out map))
            {
                map = new Dictionary<string, List<IEventsScanner>>();
                _settings.Set(WellKnownConfigurationKeys.Topology.Publishers, map);
            }

            if (!map.ContainsKey(publisherName))
                map[publisherName] = new List<IEventsScanner>();

            map[publisherName].Add(scanner);
        }
    }
}
