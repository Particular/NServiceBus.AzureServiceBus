namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Linq;
    using Addressing;
    using NServiceBus.ObjectBuilder;
    using Settings;

    public class BasicTopology : ITopology
    {
        private SettingsHolder settings;
        private IBuilder builder;

        public void InitializeSettings(SettingsHolder s)
        {
            this.settings = s;

            // apply all configuration defaults
            new DefaultConfigurationValues().Apply(settings);
            // ensures settings are present/correct
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Composition.Strategy, typeof(FlatCompositionStrategy));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Individualization.Strategy, typeof(DiscriminatorBasedIndividualizationStrategy));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Strategy, typeof(SingleNamespacePartitioningStrategy));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Strategy, typeof(AdjustmentSanitizationStrategy));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Validation.Strategy, typeof(EntityNameValidationRules));
        }

        public void InitializeContainer(IConfigureComponents container)
        {
            // runtime components
            container.ConfigureComponent(typeof(NamespaceManagerCreator), DependencyLifecycle.SingleInstance);
            container.ConfigureComponent(typeof(NamespaceManagerLifeCycleManager), DependencyLifecycle.SingleInstance);
            container.ConfigureComponent(typeof(MessagingFactoryCreator), DependencyLifecycle.SingleInstance);
            container.ConfigureComponent(typeof(MessagingFactoryLifeCycleManager), DependencyLifecycle.SingleInstance);
            container.ConfigureComponent(typeof(MessageReceiverCreator), DependencyLifecycle.SingleInstance);
            container.ConfigureComponent(typeof(MessageSenderCreator), DependencyLifecycle.SingleInstance);
            container.ConfigureComponent(typeof(MessageReceiverLifeCycleManager), DependencyLifecycle.SingleInstance);
            container.ConfigureComponent(typeof(MessageSenderLifeCycleManager), DependencyLifecycle.SingleInstance);
            container.ConfigureComponent(typeof(AzureServiceBusQueueCreator), DependencyLifecycle.SingleInstance);
            container.ConfigureComponent(typeof(AzureServiceBusTopicCreator), DependencyLifecycle.SingleInstance);
            container.ConfigureComponent(typeof(AzureServiceBusSubscriptionCreator), DependencyLifecycle.SingleInstance);
            container.ConfigureComponent(typeof(DefaultBrokeredMessagesToIncomingMessagesConverter), DependencyLifecycle.InstancePerCall);
            container.ConfigureComponent(typeof(TopologyCreator), DependencyLifecycle.InstancePerCall);
            container.ConfigureComponent(typeof(TopologyOperator), DependencyLifecycle.SingleInstance);
            container.ConfigureComponent(typeof(MessageReceiverNotifier), DependencyLifecycle.InstancePerCall);

            // strategies
            var compositionStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Composition.Strategy);
            container.ConfigureComponent(compositionStrategyType, DependencyLifecycle.InstancePerCall);

            var individualizationStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Individualization.Strategy);
            container.ConfigureComponent(individualizationStrategyType, DependencyLifecycle.InstancePerCall);

            var partitioningStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Strategy);
            container.ConfigureComponent(partitioningStrategyType, DependencyLifecycle.InstancePerCall);

            var sanitizationStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Strategy);
            container.ConfigureComponent(sanitizationStrategyType, DependencyLifecycle.InstancePerCall);

            var validationStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Validation.Strategy);
            container.ConfigureComponent(validationStrategyType, DependencyLifecycle.InstancePerCall);
        }

        public void UseBuilder(IBuilder builder)
        {
            this.builder = builder;
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

            var partitioningStrategy = (INamespacePartitioningStrategy)builder.Build(typeof(INamespacePartitioningStrategy));
            var sanitizationStrategy = (ISanitizationStrategy)builder.Build(typeof(ISanitizationStrategy));

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
            var partitioningStrategy = (INamespacePartitioningStrategy)builder.Build(typeof(INamespacePartitioningStrategy));
            var sanitizationStrategy = (ISanitizationStrategy)builder.Build(typeof(ISanitizationStrategy));

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