namespace NServiceBus
{
    using Settings;

    /// <summary>Forwarding topology specific settings.</summary>
    public class AzureServiceBusForwardingTopologySettings : TransportExtensions<AzureServiceBusTransport>
    {
        internal AzureServiceBusForwardingTopologySettings(SettingsHolder settings) : base(settings)
        {
        }
    }
}