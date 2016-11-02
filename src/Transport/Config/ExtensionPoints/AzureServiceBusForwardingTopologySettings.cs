namespace NServiceBus
{
    using Settings;

    public class AzureServiceBusForwardingTopologySettings : TransportExtensions<AzureServiceBusTransport>
    {
        internal AzureServiceBusForwardingTopologySettings(SettingsHolder settings) : base(settings)
        {
        }
    }
}