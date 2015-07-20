namespace NServiceBus
{
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Configuration.AdvanceExtensibility;

    public static class AzureServiceBusValidationExtensions
    {
        public static AzureServiceBusValidationSettings Strategy<T>(this AzureServiceBusValidationSettings validationSettings) where T : IValidationStrategy
        {
            validationSettings.GetSettings().Set(WellKnownConfigurationKeys.Topology.Addressing.Validation.Strategy, typeof(T));

            return validationSettings;
        }
    }
}