namespace NServiceBus
{
    using Settings;

    public class AzureServiceBusEndpointOrientedTopologySettings : TransportExtensions<AzureServiceBusTransport>
    {
        internal AzureServiceBusEndpointOrientedTopologySettings(SettingsHolder settings) : base(settings)
        {
        }
    }
}