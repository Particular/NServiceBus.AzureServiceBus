namespace NServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.DelayedDelivery;
    using NServiceBus.Features;
    using NServiceBus.Performance.TimeToBeReceived;
    using NServiceBus.Routing;
    using NServiceBus.Settings;
    using NServiceBus.Transports;

    public class AzureServiceBusTransport : TransportDefinition
    {
        protected override TransportReceivingConfigurationResult ConfigureForReceiving(TransportReceivingConfigurationContext context)
        {
            EnsureConnectionStringIsRegisteredAsNamespace(context.ConnectionString, context.Settings);
            return new TransportReceivingConfigurationResult(
                Topology.GetMessagePumpFactory(),
                Topology.GetQueueCreatorFactory(),
                () =>
                {
                    // Useless for me: when we resolve INamespacePartitioningStrategy to apply startup check, we have already applied checks defined into ctor of concrete classes. Right?
                    var namespacePartitioningStrategy = Container.Resolve<INamespacePartitioningStrategy>();
                    var namespaces = context.Settings.Get<List<string>>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);
                    var endpointName = context.Settings.Get<EndpointName>();

                    var configuredNamespaces = namespacePartitioningStrategy.GetNamespaces(endpointName.ToString(), PartitioningIntent.Creating);

                    if (namespaces.Count != configuredNamespaces.Count())
                        return Task.FromResult(StartupCheckResult.Failed("..."));

                    var namespaceManager  = Container.Resolve<IManageNamespaceManagerLifeCycle>()
                        .Get(context.ConnectionString);
                    if (context.Settings.Get<bool>(WellKnownConfigurationKeys.Core.CreateTopology) && !namespaceManager.HasManageRights)
                        return Task.FromResult(StartupCheckResult.Failed("..."));

                    return Task.FromResult(StartupCheckResult.Success);
                } //TODO: figure out what this is for
                );
        }

        protected override TransportSendingConfigurationResult ConfigureForSending(TransportSendingConfigurationContext context)
        {
            EnsureConnectionStringIsRegisteredAsNamespace(context.ConnectionString, context.Settings);
            return new TransportSendingConfigurationResult(
                Topology.GetDispatcherFactory(),
                () => Task.FromResult(StartupCheckResult.Success) //TODO: figure out what this is for
                );
        }

        private void EnsureConnectionStringIsRegisteredAsNamespace(string connectionstring, ReadOnlySettings settings)
        {
            var namespaces = settings.Get<List<string>>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);
            if (namespaces.All(n => n != connectionstring))
            {
                namespaces.Add(connectionstring);
            }
        }

        public override IEnumerable<Type> GetSupportedDeliveryConstraints()
        {
            return new List<Type>
            {
                typeof(DelayDeliveryWith),
                typeof(DoNotDeliverBefore),
                typeof(DiscardIfNotReceivedBefore),
            };
        }

        public override TransportTransactionMode GetSupportedTransactionMode()
        {
            // TODO: See where Core is calling this and make sure None is the correct value.
            // TODO: TransportTransactionMode may need to be dependent upon topology.
            return TransportTransactionMode.SendsAtomicWithReceive;
        }
        
        public override IManageSubscriptions GetSubscriptionManager()
        {
            return Topology.GetSubscriptionManager();
        }

        public override EndpointInstance BindToLocalEndpoint(EndpointInstance instance, ReadOnlySettings settings)
        {
            // ???
            return instance;
        }

        public override string ToTransportAddress(LogicalAddress logicalAddress)
        {
            // ???
            return logicalAddress.ToString();
        }

        public override OutboundRoutingPolicy GetOutboundRoutingPolicy(ReadOnlySettings settings)
        {
            return TopologyWithScenarioDescriptorFallback.GetOutboundRoutingPolicy();
        }

        public override string ExampleConnectionStringForErrorMessage { get; } = "Endpoint=sb://[namespace].servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=[secret_key]";

        private ITopology _topology;

#pragma warning disable 649
        private static Func<ITopology> _fallbackForScenarioDescriptors; 
#pragma warning restore 649

        internal IResolveTransportParts Container { get; set; }
           
        internal ITopology Topology
        {
            get
            {
                return _topology;
            }
            set { _topology = value; }
        }

        private ITopology TopologyWithScenarioDescriptorFallback
        {
            get
            {
                // acceptence tests scenario descriptors use an uninitialized transportdefinition
                // in order to detect whether a test should run or not, to make this work
                // we need to make sure it gets the correct topology assigned when starting a testrun
                // once the real topology is set, it should use that instead
                if (Topology == null && _fallbackForScenarioDescriptors != null)
                {
                    return _fallbackForScenarioDescriptors();
                }

                return Topology;
            }
        }
    }

    public class AzureServiceBusTransportConfigurator : Feature
    {
        private SettingsHolder settings { get; set; }

        internal AzureServiceBusTransportConfigurator()
        {
            EnableByDefault();
            //DependsOn<UnicastBus>();
            //DependsOn<Receiving>();
            Defaults(settings =>
            {
                this.settings = settings;
                settings.SetDefault("Transactions.DoNotWrapHandlersExecutionInATransactionScope", true);
                settings.SetDefault("Transactions.SuppressDistributedTransactions", true);
                settings.SetDefault<ITopology>(new StandardTopology());

                var transportDefinition = (AzureServiceBusTransport) settings.Get<TransportDefinition>();
                if (transportDefinition.Topology == null)
                {
                    transportDefinition.Topology = settings.Get<ITopology>(); 
                }

                transportDefinition.Container = transportDefinition.Topology.Initialize(settings);
            });
        }

        


        protected override void Setup(FeatureConfigurationContext context)
        {
            //context.Container //can only register
            //context.Pipeline //can extend
            //context.Settings //cannot change

        }


    }

}