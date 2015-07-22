namespace NServiceBus
{
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;

    public class AzureServiceBusSanitizationSettings : ExposeSettings
    {
         SettingsHolder _settings;

         public AzureServiceBusSanitizationSettings(SettingsHolder settings)
            : base(settings)
        {
            _settings = settings;
        }

         public AzureServiceBusSanitizationSettings UseStrategy<T>() where T : ISanitizationStrategy
         {
             _settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Strategy, typeof(T));

             return this;
         }
    }
}