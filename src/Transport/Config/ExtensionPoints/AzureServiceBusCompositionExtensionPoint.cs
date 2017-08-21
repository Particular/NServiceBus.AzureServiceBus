namespace NServiceBus
{
    using Configuration.AdvancedExtensibility;
    using Settings;
    using Transport.AzureServiceBus;

    public class AzureServiceBusCompositionExtensionPoint<T> : ExposeSettings where T : ICompositionStrategy
    {
        internal AzureServiceBusCompositionExtensionPoint(SettingsHolder settings) : base(settings)
        {
        }
    }
}