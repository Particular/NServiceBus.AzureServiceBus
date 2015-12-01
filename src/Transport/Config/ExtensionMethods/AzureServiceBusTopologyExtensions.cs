namespace NServiceBus
{
    using NServiceBus.Configuration.AdvanceExtensibility;

    public static class AzureServiceBusTopologyExtensions
    {
        public static AzureServiceBusResourceSettings Resources(this AzureServiceBusTopologySettings topologySettings)
        {
            return new AzureServiceBusResourceSettings(topologySettings.GetSettings());
        }

        public static AzureServiceBusAddressingSettings Addressing(this AzureServiceBusTopologySettings topologySettings)
        {
            return new AzureServiceBusAddressingSettings(topologySettings.GetSettings());
        }
    }
}
