namespace NServiceBus
{
    using NServiceBus.AzureServiceBus;
    using NServiceBus.Settings;
    using NServiceBus.Transports;

    public class AzureServiceBusTransport : TransportDefinition
    {
        protected override TransportInfrastructure Initialize(SettingsHolder settings)
        {
            settings.SetDefault("Transactions.DoNotWrapHandlersExecutionInATransactionScope", true);
            settings.SetDefault("Transactions.SuppressDistributedTransactions", true);
            settings.SetDefault<ITopology>(new StandardTopology());

            var topology = settings.Get<ITopology>();
            topology.Initialize(settings);

            return new AzureServiceBusTransportInfrastructure(topology, settings);
        }
    }
}