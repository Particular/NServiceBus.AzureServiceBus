namespace NServiceBus
{
    using System;
    using AzureServiceBus.Topology.MetaModel;
    using Logging;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Serialization;
    using Settings;
    using Transport;
    using Transport.AzureServiceBus;

    public class AzureServiceBusTransport : TransportDefinition
    {
        static ILog logger = LogManager.GetLogger(typeof(AzureServiceBusTransport));

        public override TransportInfrastructure Initialize(SettingsHolder settings, string connectionString)
        {
            // override core default serialization
            settings.SetDefault(WellKnownConfigurationKeys.Core.MainSerializerSettingsKey, Tuple.Create<SerializationDefinition, SettingsHolder>(new JsonSerializer(), new SettingsHolder()));


            var topology = GetConfiguredTopology(settings);
            topology.Initialize(settings);

            if (!string.IsNullOrEmpty(connectionString))
            {
                RegisterConnectionStringAsNamespace(connectionString, settings);
            }

            MatchSettingsToConsistencyRequirements(settings);

            SetConnectivityMode(settings);

            return new AzureServiceBusTransportInfrastructure(topology, settings.SupportedTransactionMode(), settings.Get<SatelliteTransportAddressCollection>());
        }

        static ITopologyInternal GetConfiguredTopology(SettingsHolder settings)
        {
            var configuredTopology = settings.GetOrDefault<ITopologyInternal>();
            if (configuredTopology == null)
            {
                throw new Exception("Azure Service Bus transport requires a topology to be specified. Use `.UseForwardingTopology()` or `.UseEndpointOrientedTopology()` configuration API to specify topology to use.");
            }
            return configuredTopology;
        }

        void MatchSettingsToConsistencyRequirements(SettingsHolder settings)
        {
            if (settings.HasSetting<TransportTransactionMode>())
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
                        // override the default for prefetch count, but user code can still take precedence
                        settings.SetDefault(WellKnownConfigurationKeys.Connectivity.MessageReceivers.PrefetchCount, 0);

                        if (!settings.HasExplicitValue(WellKnownConfigurationKeys.Connectivity.MessageReceivers.PrefetchCount))
                        {
                            logger.Warn("Default value for message receiver's PrefetchCount was reduced to zero to avoid message loss with ReceiveAndDelete receive mode. To enforce prefetch, use the configuration API to set the value explicitly.");
                        }
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
            var namespaces = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
            var alias = settings.Get<string>(WellKnownConfigurationKeys.Topology.Addressing.DefaultNamespaceAlias);
            namespaces.Add(alias, connectionString, NamespacePurpose.Partitioning);
        }

        public override bool RequiresConnectionString { get; } = false;

        public override string ExampleConnectionStringForErrorMessage { get; } = "Endpoint=sb://[namespace].servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=[secret_key]";
    }
}