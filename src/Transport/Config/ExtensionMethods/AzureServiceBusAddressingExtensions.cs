namespace NServiceBus
{
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Configuration.AdvanceExtensibility;

    public static class AzureServiceBusAddressingExtensions
    {
        public static AzureServiceBusAddressingSettings UseStrategy<T>(this AzureServiceBusAddressingSettings addressingSettings) where T : IAddressingStrategy
        {
            addressingSettings.GetSettings().Set(WellKnownConfigurationKeys.Topology.Addressing.Strategy, typeof(T));

            return addressingSettings;
        }

        public static AzureServiceBusNamespacePartitioningSettings NamespacePartitioning(this AzureServiceBusAddressingSettings addressingSettings)
        {
            return new AzureServiceBusNamespacePartitioningSettings(addressingSettings.GetSettings());
        }

        public static AzureServiceBusCompositionSettings Composition(this AzureServiceBusAddressingSettings addressingSettings)
        {
            return new AzureServiceBusCompositionSettings(addressingSettings.GetSettings());
        }

        public static AzureServiceBusValidationSettings Validation(this AzureServiceBusAddressingSettings addressingSettings)
        {
            return new AzureServiceBusValidationSettings(addressingSettings.GetSettings());
        }

        public static AzureServiceBusSanitizationSettings Sanitization(this AzureServiceBusAddressingSettings addressingSettings)
        {
            return new AzureServiceBusSanitizationSettings(addressingSettings.GetSettings());
        }

        public static AzureServiceBusIndividualizationSettings Individualization(this AzureServiceBusAddressingSettings addressingSettings)
        {
            return new AzureServiceBusIndividualizationSettings(addressingSettings.GetSettings());
        } 
    }
}
