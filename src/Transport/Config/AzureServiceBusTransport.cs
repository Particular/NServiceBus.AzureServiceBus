namespace NServiceBus
{
    using System;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using AzureServiceBus;
    using Serialization;
    using Settings;
    using Transports;

    public class AzureServiceBusTransport : TransportDefinition
    {
        protected override TransportInfrastructure Initialize(SettingsHolder settings, string connectionString)
        {
            settings.SetDefault("Transactions.DoNotWrapHandlersExecutionInATransactionScope", true);
            settings.SetDefault("Transactions.SuppressDistributedTransactions", true);
            settings.SetDefault<ITopology>(new EndpointOrientedTopology());
            // override core default serialization
            settings.SetDefault<SerializationDefinition>(new JsonSerializer());

            var topology = settings.Get<ITopology>();
            topology.Initialize(settings);

            RegisterConnectionStringAsNamespace(connectionString, settings);

            MatchSettingsToConsistencyRequirements(settings);

            SetConnectivityMode(settings);

            return new AzureServiceBusTransportInfrastructure(topology, settings);
        }

        void MatchSettingsToConsistencyRequirements(SettingsHolder settings)
        {
            if (settings.HasExplicitValue<TransportTransactionMode>())
            {
                var required = settings.Get<TransportTransactionMode>();
                if (required > TransportTransactionMode.SendsAtomicWithReceive)
                {
                    throw new InvalidOperationException($"Azure Service Bus transport doesn't support the required transaction mode {required}.");
                }
                if (required > settings.SupportedTransactionMode())
                {
                    throw new InvalidOperationException($"Azure Service Bus transport doesn't support the required transaction mode {required}, for the given configuration settings.");
                }
                if (required < settings.SupportedTransactionMode())
                {
                    if (required < TransportTransactionMode.SendsAtomicWithReceive)
                    {
                        // turn send via off so that sends are not atomic
                        settings.Set(WellKnownConfigurationKeys.Connectivity.SendViaReceiveQueue, false);
                    }

                    if (required == TransportTransactionMode.None)
                    {
                        // immediately delete after receive
                        settings.Set(WellKnownConfigurationKeys.Connectivity.MessageReceivers.ReceiveMode, ReceiveMode.ReceiveAndDelete);
                    }

                }
            }
        }

        static void SetConnectivityMode(SettingsHolder settings)
        {
            ServiceBusEnvironment.SystemConnectivity.Mode = settings.Get<ConnectivityMode>(WellKnownConfigurationKeys.Connectivity.ConnectivityMode);
        }

        static void RegisterConnectionStringAsNamespace(string connectionString, ReadOnlySettings settings)
        {
            var namespaces = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);
            namespaces.AddDefault(connectionString);
        }

        public override bool RequiresConnectionString { get; } = true;

        public override string ExampleConnectionStringForErrorMessage { get; } = "Endpoint=sb://[namespace].servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=[secret_key]";
    }

}