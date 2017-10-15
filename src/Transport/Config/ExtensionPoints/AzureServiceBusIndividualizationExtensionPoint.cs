namespace NServiceBus
{
    using Configuration.AdvancedExtensibility;
    using Settings;
    using Transport.AzureServiceBus;

    /// <summary><see cref="IIndividualizationStrategy"/> related settings extension point.</summary>
    public class AzureServiceBusIndividualizationExtensionPoint<T> : ExposeSettings where T : IIndividualizationStrategy
    {
        internal AzureServiceBusIndividualizationExtensionPoint(SettingsHolder settings) : base(settings)
        {
        }
    }
}