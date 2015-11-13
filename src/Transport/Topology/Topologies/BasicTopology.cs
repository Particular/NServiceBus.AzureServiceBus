namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Linq;
    using Addressing;
    using ObjectBuilder.Common;
    using Settings;

    public class BasicTopology : ITopology
    {
        readonly SettingsHolder settings;
        readonly IContainer container;

        public BasicTopology(SettingsHolder settings, IContainer container)
        {
            this.settings = settings;
            this.container = container;
        }

        public void InitializeSettings()
        {
            // apply all configuration defaults
            new DefaultConfigurationValues().Apply(settings);
            // ensures settings are present/correct
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Composition.Strategy, typeof(FlatCompositionStrategy));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Individualization.Strategy, typeof(DiscriminatorBasedIndividualizationStrategy));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Strategy, typeof(SingleNamespacePartitioningStrategy));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Strategy, typeof(AdjustmentSanitizationStrategy));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Validation.Strategy, typeof(EntityNameValidationRules));
        }

        public void InitializeContainer()
        {
            // runtime components
            container.Configure(typeof(NamespaceManagerCreator), DependencyLifecycle.SingleInstance);
            container.Configure(typeof(NamespaceManagerLifeCycleManager), DependencyLifecycle.SingleInstance);
            container.Configure(typeof(MessagingFactoryCreator), DependencyLifecycle.SingleInstance);
            container.Configure(typeof(MessagingFactoryLifeCycleManager), DependencyLifecycle.SingleInstance);
            container.Configure(typeof(MessageReceiverCreator), DependencyLifecycle.SingleInstance);
            container.Configure(typeof(MessageSenderCreator), DependencyLifecycle.SingleInstance);
            container.Configure(typeof(MessageReceiverLifeCycleManager), DependencyLifecycle.SingleInstance);
            container.Configure(typeof(MessageSenderLifeCycleManager), DependencyLifecycle.SingleInstance);
            container.Configure(typeof(AzureServiceBusQueueCreator), DependencyLifecycle.SingleInstance);
            container.Configure(typeof(AzureServiceBusTopicCreator), DependencyLifecycle.SingleInstance);
            container.Configure(typeof(AzureServiceBusSubscriptionCreator), DependencyLifecycle.SingleInstance);
            container.Configure(typeof(DefaultBrokeredMessagesToIncomingMessagesConverter), DependencyLifecycle.InstancePerCall);
            container.Configure(typeof(TopologyCreator), DependencyLifecycle.InstancePerCall);
            container.Configure(typeof(TopologyOperator), DependencyLifecycle.SingleInstance);
            container.Configure(typeof(MessageReceiverNotifier), DependencyLifecycle.InstancePerCall);

            // strategies
            var compositionStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Composition.Strategy);
            container.Configure(compositionStrategyType, DependencyLifecycle.InstancePerCall);

            var individualizationStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Individualization.Strategy);
            container.Configure(individualizationStrategyType, DependencyLifecycle.InstancePerCall);

            var partitioningStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Strategy);
            container.Configure(partitioningStrategyType, DependencyLifecycle.InstancePerCall);

            var sanitizationStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Strategy);
            container.Configure(sanitizationStrategyType, DependencyLifecycle.InstancePerCall);

            var validationStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Validation.Strategy);
            container.Configure(validationStrategyType, DependencyLifecycle.InstancePerCall);
        }

        public TopologySection DetermineReceiveResources()
        {
            return Determine(PartitioningIntent.Receiving);
        }

        public TopologySection DetermineResourcesToCreate()
        {
            return Determine(PartitioningIntent.Creating);
        }

        private TopologySection Determine(PartitioningIntent partitioningIntent)
        {
            // computes the topology

            var endpointName = settings.Get<EndpointName>();

            var partitioningStrategy = (INamespacePartitioningStrategy)container.Build(typeof(INamespacePartitioningStrategy));
            var sanitizationStrategy = (ISanitizationStrategy)container.Build(typeof(ISanitizationStrategy));

            var namespaces = partitioningStrategy.GetNamespaces(endpointName.ToString(), partitioningIntent).ToArray();

            var inputQueuePath = sanitizationStrategy.Sanitize(endpointName.ToString(), EntityType.Queue);
            var inputQueues = namespaces.Select(n => new EntityInfo { Path = inputQueuePath, Type = EntityType.Queue, Namespace = n }).ToArray();

            //TODO: core has a a list of queues as well, which I suppose includes ErrorQ & AuditQ
            // integrate those correctly into the topology
            // settings.Get<QueueBindings>()

            var entities = inputQueues.ToArray();

            return new TopologySection
            {
                Namespaces = namespaces,
                Entities = entities
            };
        }

        public TopologySection DeterminePublishDestination(Type eventType)
        {
            throw new NotSupportedException("The current topology does not support publishing via azure servicebus directly");
        }

        public TopologySection DetermineSendDestination(string destination)
        {
            var partitioningStrategy = (INamespacePartitioningStrategy)container.Build(typeof(INamespacePartitioningStrategy));
            var sanitizationStrategy = (ISanitizationStrategy)container.Build(typeof(ISanitizationStrategy));

            var namespaces = partitioningStrategy.GetNamespaces(destination, PartitioningIntent.Sending).ToArray();

            var destinationQueuePath = sanitizationStrategy.Sanitize(destination, EntityType.Queue);
            var destinationQueues = namespaces.Select(n => new EntityInfo { Path = destinationQueuePath, Type = EntityType.Queue, Namespace = n }).ToArray();

            return new TopologySection
            {
                Namespaces = namespaces,
                Entities = destinationQueues
            };
        }

        public TopologySection DetermineResourcesToSubscribeTo(Type eventType)
        {
            throw new NotSupportedException("The current topology does not support azure servicebus subscriptions");
        }

        public TopologySection DetermineResourcesToUnsubscribeFrom(Type eventtype)
        {
            throw new NotSupportedException("The current topology does not support azure servicebus subscriptions");
        }
    }
}