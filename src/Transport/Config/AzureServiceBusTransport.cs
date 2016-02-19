namespace NServiceBus
{
    using System.Collections.Generic;
    using System.Linq;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.Settings;
    using NServiceBus.Transports;

    public class AzureServiceBusTransport : TransportDefinition
    {
        protected override TransportInfrastructure Initialize(SettingsHolder settings, string connectionString)
        {
            settings.SetDefault("Transactions.DoNotWrapHandlersExecutionInATransactionScope", true);
            settings.SetDefault("Transactions.SuppressDistributedTransactions", true);
            settings.SetDefault<ITopology>(new StandardTopology());

            var topology = settings.Get<ITopology>();
            topology.Initialize(settings);

            RegisterConnectionStringAsNamespace(connectionString, settings);

            return new AzureServiceBusTransportInfrastructure(topology, settings);
        }

        private void RegisterConnectionStringAsNamespace(string connectionstring, ReadOnlySettings settings)
        {
            var namespaces = settings.Get<List<string>>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);
            if (namespaces.All(n => n != connectionstring))
            {
                namespaces.Add(connectionstring);
            }
        }

        public override bool RequiresConnectionString { get; } = true;

        public override string ExampleConnectionStringForErrorMessage { get; } = "Endpoint=sb://[namespace].servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=[secret_key]";
    }

}