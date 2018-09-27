namespace NServiceBus
{
    using Settings;

    /// <summary>Endpoint-Oriented topology specific settings.</summary>
    public class AzureServiceBusEndpointOrientedTopologySettings : TransportExtensions<AzureServiceBusTransport>
    {
        internal AzureServiceBusEndpointOrientedTopologySettings(SettingsHolder settings) : base(settings)
        {
        }
    }
}