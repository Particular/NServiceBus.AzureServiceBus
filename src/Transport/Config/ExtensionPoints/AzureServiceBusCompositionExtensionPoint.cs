namespace NServiceBus
{
    using Configuration.AdvancedExtensibility;
    using Settings;
    using Transport.AzureServiceBus;

    /// <summary><see cref="ICompositionStrategy"/> related settings extension point.</summary>
    public class AzureServiceBusCompositionExtensionPoint<T> : ExposeSettings where T : ICompositionStrategy
    {
        internal AzureServiceBusCompositionExtensionPoint(SettingsHolder settings) : base(settings)
        {
        }
    }
}