namespace NServiceBus
{
    using System;
    using AzureServiceBus;
    using Logging;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Settings;
    using Transport;
    using Transport.AzureServiceBus;

    /// <summary>Transport definition for Azure Service Bus.</summary>
    public class AzureServiceBusTransport : TransportDefinition
    {
        /// <summary>
        /// <see cref="TransportDefinition.ExampleConnectionStringForErrorMessage" />.
        /// </summary>
        public override bool RequiresConnectionString { get; } = false;

        /// <summary>
        /// <see cref="TransportDefinition.ExampleConnectionStringForErrorMessage" />.
        /// </summary>
        public override string ExampleConnectionStringForErrorMessage { get; } = "Endpoint=sb://[namespace].servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=[secret_key]";

        /// <summary>
        /// Initializes the transport infrastructure for Azure Service Bus.
        /// </summary>
        /// <returns>the transport infrastructure forAzure Service Bus.</returns>
        public override TransportInfrastructure Initialize(SettingsHolder settings, string connectionString)
        {
            DefaultConfigurationValues.Apply(settings);

            if (!string.IsNullOrEmpty(connectionString))
            {
                RegisterConnectionStringAsNamespace(connectionString, settings);
            }

            MatchSettingsToConsistencyRequirements(settings);
            SetConnectivityMode(settings);

            return CreateTransportInfrastructure(settings);
        }

        static TransportInfrastructure CreateTransportInfrastructure(SettingsHolder settings)
        {
            settings.TryGet(WellKnownConfigurationKeys.Topology.Selected, out string configuredTopology);
            switch (configuredTopology)
            {
                case WellKnownConfigurationKeys.Topology.EndpointOrientedTopology:
                    return new EndpointOrientedTransportInfrastructure(settings);
                case WellKnownConfigurationKeys.Topology.ForwardingTopology:
                    return new ForwardingTransportInfrastructure(settings);
                default:
                    throw new Exception("Azure Service Bus transport requires a topology to be specified. Use `.UseForwardingTopology()` or `.UseEndpointOrientedTopology()` configuration API to specify topology to use.");
            }
        }

        static void MatchSettingsToConsistencyRequirements(SettingsHolder settings)
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

        static ILog logger = LogManager.GetLogger(typeof(AzureServiceBusTransport));
    }
}