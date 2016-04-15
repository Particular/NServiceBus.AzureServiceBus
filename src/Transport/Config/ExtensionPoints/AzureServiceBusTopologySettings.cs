namespace NServiceBus
{
    using AzureServiceBus;
    using Settings;

    public class AzureServiceBusTopologySettings<T> : TransportExtensions<AzureServiceBusTransport> where T : ITopology
    {
        public AzureServiceBusTopologySettings(SettingsHolder settings) : base(settings)
        {
        }
    }
}
