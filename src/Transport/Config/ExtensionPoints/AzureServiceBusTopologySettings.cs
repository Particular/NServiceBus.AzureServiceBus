namespace NServiceBus
{
    using NServiceBus.Settings;

    public class AzureServiceBusTopologySettings : TransportExtensions<AzureServiceBusTransport>
    {
        public AzureServiceBusTopologySettings(SettingsHolder settings)
            : base(settings)
        {
        }
    }
}
