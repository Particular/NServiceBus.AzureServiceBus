namespace NServiceBus
{
    using Settings;
    using Transport.AzureServiceBus;

    public class AzureServiceBusTopologySettings<T> : TransportExtensions<AzureServiceBusTransport> where T : ITopology
    {
        public AzureServiceBusTopologySettings(SettingsHolder settings) : base(settings)
        {
        }
    }
}
