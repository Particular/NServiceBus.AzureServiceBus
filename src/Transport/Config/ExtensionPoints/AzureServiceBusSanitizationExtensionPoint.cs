namespace NServiceBus
{
    using System;
    using AzureServiceBus;
    using AzureServiceBus.Addressing;
    using Configuration.AdvanceExtensibility;
    using Settings;

    public class AzureServiceBusSanitizationExtensionPoint<T> : ExposeSettings where T : ISanitizationStrategy
    {
        SettingsHolder settings;

        public AzureServiceBusSanitizationExtensionPoint(SettingsHolder settings) : base(settings)
        {
            this.settings = settings;
        }

        public AzureServiceBusSanitizationExtensionPoint<T> Hash(Func<string, string> hash)
        {
            Guard.AgainstNull(nameof(hash), hash);
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Hash, hash);
            return this;
        }
    }
}