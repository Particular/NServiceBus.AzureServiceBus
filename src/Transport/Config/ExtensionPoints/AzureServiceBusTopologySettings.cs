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
            var settings = this.GetSettings();
            var transportDefinition = settings.Get<TransportDefinition>() as AzureServiceBusTransport;

            if (transportDefinition != null)
            {
                var topology = Activator.CreateInstance<T>();
                transportDefinition.Topology = topology;
            }

            return this;
        }
    }
}
