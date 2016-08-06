namespace NServiceBus
{
    using Configuration.AdvanceExtensibility;
    using Settings;
    using Transport.AzureServiceBus;

    public class AzureServiceBusIndividualizationExtensionPoint<T> : ExposeSettings where T : IIndividualizationStrategy
    {
        public AzureServiceBusIndividualizationExtensionPoint(SettingsHolder settings) : base(settings)
        {
        }
    }
}