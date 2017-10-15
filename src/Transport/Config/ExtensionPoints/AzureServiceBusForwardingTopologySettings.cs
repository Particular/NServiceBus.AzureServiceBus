namespace NServiceBus
{
    using Settings;

    /// <summary><see cref="ForwardingTopology"/> specific settings.</summary>
    public class AzureServiceBusForwardingTopologySettings : TransportExtensions<AzureServiceBusTransport>
    {
        internal AzureServiceBusForwardingTopologySettings(SettingsHolder settings) : base(settings)
        {
        }
    }
}