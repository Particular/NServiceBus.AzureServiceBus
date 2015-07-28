namespace NServiceBus
{
    using System;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusResourceSettings : ExposeSettings
    {

        public AzureServiceBusResourceSettings(SettingsHolder settings)
            : base(settings)
        {
        }

    }
}
