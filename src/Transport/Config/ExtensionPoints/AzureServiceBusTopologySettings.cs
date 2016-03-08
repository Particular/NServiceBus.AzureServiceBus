namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.Settings;

    public class AzureServiceBusTopologySettings : TransportExtensions<AzureServiceBusTransport>
    {
        private readonly SettingsHolder _settings;

        public AzureServiceBusTopologySettings(SettingsHolder settings)
            : base(settings)
        {
            _settings = settings;
        }

        public AzureServiceBusTopologySettings RegisterPublisherFor<T>(string publisher)
        {
            Dictionary<Type, List<string>> map;

            if (!_settings.TryGet(WellKnownConfigurationKeys.Topology.Publishers, out map))
            {
                map = new Dictionary<Type, List<string>>();
                _settings.Set(WellKnownConfigurationKeys.Topology.Publishers, map);
            }

            if (!map.ContainsKey(typeof(T)))
                map[typeof(T)] = new List<string>();

            var publishers = map[typeof(T)];
            publishers.Add(publisher);

            return this;
        }
    }
}
