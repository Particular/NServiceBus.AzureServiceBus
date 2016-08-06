namespace NServiceBus
{
    using Configuration.AdvanceExtensibility;
    using Transport.AzureServiceBus;
    using Settings;

    public class AzureServiceBusCompositionExtensionPoint<T> : ExposeSettings where T : ICompositionStrategy
    {
        public AzureServiceBusCompositionExtensionPoint(SettingsHolder settings) : base(settings)
        {
        }
    }
}