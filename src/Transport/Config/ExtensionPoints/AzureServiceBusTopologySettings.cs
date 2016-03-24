namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.AzureServiceBus.TypesScanner;
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
            EnsureThatConfiguredTopologyNeedsMappingConfigurationBetweenPublishersAndEventTypes();

            AddScannerForPublisher(publisherName, new SingleTypeScanner(type));
            return this;
        }

        public AzureServiceBusTopologySettings RegisterPublisherForAssembly(string publisherName, Assembly assembly)
        {
            EnsureThatConfiguredTopologyNeedsMappingConfigurationBetweenPublishersAndEventTypes();

            AddScannerForPublisher(publisherName, new AssemblyTypesScanner(assembly));
            return this;
        }

        private void AddScannerForPublisher(string publisherName, ITypesScanner scanner)
        {
            Dictionary<string, List<ITypesScanner>> map;

            if (!_settings.TryGet(WellKnownConfigurationKeys.Topology.Publishers, out map))
            {
                map = new Dictionary<string, List<ITypesScanner>>();
                _settings.Set(WellKnownConfigurationKeys.Topology.Publishers, map);
            }

            if (!map.ContainsKey(publisherName))
                map[publisherName] = new List<ITypesScanner>();

            map[publisherName].Add(scanner);
        }

        private void EnsureThatConfiguredTopologyNeedsMappingConfigurationBetweenPublishersAndEventTypes()
        {
            var topology = _settings.Get<ITopology>();

            if (!topology.NeedsMappingConfigurationBetweenPublishersAndEventTypes)
                throw new InvalidOperationException($"Configured topology (`{topology.GetType().FullName}`) doesn't need mapping configuration between publisher names and event types");
        }
    }
}
