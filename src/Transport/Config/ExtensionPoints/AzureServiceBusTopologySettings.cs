namespace NServiceBus
{
    using Settings;
    using Transport.AzureServiceBus;

    // TODO: review how we deal with this. Either break out into two topology specific settings classes or leave as one
    public class AzureServiceBusTopologySettings<T> : TransportExtensions<AzureServiceBusTransport> where T : ITopology
    {
        public AzureServiceBusTopologySettings(SettingsHolder settings) : base(settings)
        {
        }
    }
}
