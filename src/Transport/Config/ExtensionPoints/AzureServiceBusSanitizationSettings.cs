namespace NServiceBus
{
    using AzureServiceBus;
    using AzureServiceBus.Addressing;
    using Configuration.AdvanceExtensibility;
    using Settings;

    public class AzureServiceBusSanitizationSettings : ExposeSettings
    {
         SettingsHolder settings;

         public AzureServiceBusSanitizationSettings(SettingsHolder settings)
            : base(settings)
        {
            this.settings = settings;
        }

        /// <summary>
        /// Rules to apply for entity path/name sanitization.
        /// <remarks> Default is <see cref="ThrowOnFailingSanitization"/>. For backwards compatibility, use <see cref="AdjustmentSanitizationV6"/>.</remarks>
        /// </summary>
        public AzureServiceBusSanitizationSettings UseStrategy<T>() where T : ISanitizationStrategy
         {
             settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Strategy, typeof(T));

             return this;
         }
    }
}