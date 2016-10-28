namespace NServiceBus
{
    using Configuration.AdvanceExtensibility;
    using Transport.AzureServiceBus;
    using Settings;

    public class AzureServiceBusCompositionExtensionPoint<T> : ExposeSettings where T : ICompositionStrategy
    {
        [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public AzureServiceBusCompositionExtensionPoint(SettingsHolder settings) : base(settings)
        {
        }
    }
}