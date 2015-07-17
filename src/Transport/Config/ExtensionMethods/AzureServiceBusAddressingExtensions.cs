namespace NServiceBus
{
    using NServiceBus.Configuration.AdvanceExtensibility;

    public static class AzureServiceBusAddressingExtensions
    {
        public static AzureServiceBusPartitioningSettings Partitioning(this AzureServiceBusAddressingSettings addressingSettings)
        {
            return new AzureServiceBusPartitioningSettings(addressingSettings.GetSettings());
        }

        public static AzureServiceBusCompositionSettings Composition(this AzureServiceBusAddressingSettings addressingSettings)
        {
            return new AzureServiceBusCompositionSettings(addressingSettings.GetSettings());
        }

        public static AzureServiceBusValidationSettings Validation(this AzureServiceBusAddressingSettings addressingSettings)
        {
            return new AzureServiceBusValidationSettings(addressingSettings.GetSettings());
        }

        public static AzureServiceBusIndividualizationSettings Individualization(this AzureServiceBusAddressingSettings addressingSettings)
        {
            return new AzureServiceBusIndividualizationSettings(addressingSettings.GetSettings());
        } 
    }
}
