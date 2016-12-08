namespace NServiceBus
{
    using Configuration.AdvanceExtensibility;
    using Settings;
    using Transport.AzureServiceBus;

    public class AzureServiceBusIndividualizationExtensionPoint<T> : ExposeSettings where T : IIndividualizationStrategy
    {
        internal AzureServiceBusIndividualizationExtensionPoint(SettingsHolder settings) : base(settings)
        {
        }
    }
}