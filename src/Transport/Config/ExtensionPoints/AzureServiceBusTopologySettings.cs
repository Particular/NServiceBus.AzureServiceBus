namespace NServiceBus
{
    using Settings;
    using Transport.AzureServiceBus;

    // TODO: review how we deal with this. Either break out into two topology specific settings classes or leave as one
    public class AzureServiceBusTopologySettings<T> : TransportExtensions<AzureServiceBusTransport> where T : ITopology
    {
        [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public AzureServiceBusTopologySettings(SettingsHolder settings) : base(settings)
        {
        }
    }
}
