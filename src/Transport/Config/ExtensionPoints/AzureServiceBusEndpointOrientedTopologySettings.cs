namespace NServiceBus
{
    using Settings;

    /// <summary><see cref="EndpointOrientedTopology"/> specific settings.</summary>
    public class AzureServiceBusEndpointOrientedTopologySettings : TransportExtensions<AzureServiceBusTransport>
    {
        internal AzureServiceBusEndpointOrientedTopologySettings(SettingsHolder settings) : base(settings)
        {
        }
    }
}