namespace NServiceBus
{
    using Settings;
    using Transport.AzureServiceBus;

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class AzureServiceBusTopologySettings<T> : TransportExtensions<AzureServiceBusTransport> where T : ITopology
    {
        [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public AzureServiceBusTopologySettings(SettingsHolder settings) : base(settings)
        {
        }
    }
}
