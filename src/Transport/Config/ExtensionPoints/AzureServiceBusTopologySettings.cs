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


    }
}
