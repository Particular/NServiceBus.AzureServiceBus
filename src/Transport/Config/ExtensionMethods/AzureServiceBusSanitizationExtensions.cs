namespace NServiceBus
{
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Configuration.AdvanceExtensibility;

    public static class AzureServiceBusSanitizationExtensions
    {
        public static AzureServiceBusSanitizationSettings Strategy<T>(this AzureServiceBusSanitizationSettings sanitizationSettings) where T : ISanitizationStrategy
        {
            sanitizationSettings.GetSettings().Set(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Strategy, typeof(T));

            return sanitizationSettings;
        }
    }
}