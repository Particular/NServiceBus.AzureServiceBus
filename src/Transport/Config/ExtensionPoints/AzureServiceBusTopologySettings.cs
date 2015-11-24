namespace NServiceBus
{
    using System;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;
    using NServiceBus.Transports;

    public class AzureServiceBusTopologySettings : ExposeSettings
    {
        public AzureServiceBusTopologySettings(SettingsHolder settings)
            : base(settings)
        {
        }

        public AzureServiceBusTopologySettings Use<T>() where T : ITopology, new()
        {
            var topology = Activator.CreateInstance<T>();
            return Use(topology);
        }
        public AzureServiceBusTopologySettings Use<T>(Func<T> factory) where T : ITopology
        {
            return Use(factory());
        }
        public AzureServiceBusTopologySettings Use<T>(T topology) where T : ITopology
        {
            var settings = this.GetSettings();
            var transportDefinition = settings.Get<TransportDefinition>() as AzureServiceBusTransport;

            if (transportDefinition != null)
            {
                transportDefinition.Topology = topology;
            }

            return this;
        }
    }
}
