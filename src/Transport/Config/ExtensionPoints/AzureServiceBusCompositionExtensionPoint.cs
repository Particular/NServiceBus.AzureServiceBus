namespace NServiceBus
{
    using AzureServiceBus.Addressing;
    using Configuration.AdvanceExtensibility;
    using Settings;

    public class AzureServiceBusCompositionExtensionPoint<T> : ExposeSettings where T : ICompositionStrategy
    {
        public AzureServiceBusCompositionExtensionPoint(SettingsHolder settings) : base(settings)
        {
        }
    }
}