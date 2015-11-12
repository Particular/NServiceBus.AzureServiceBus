namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Addressing;
    using NServiceBus.ObjectBuilder;
    using Settings;

    public class StandardTopology : ITopology
    {
        private SettingsHolder settings;
        private IBuilder builder;

        readonly ConcurrentDictionary<Type, TopologySection> subscriptions = new ConcurrentDictionary<Type, TopologySection>();

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
            // configures container
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

            var topicPath = sanitizationStrategy.Sanitize(endpointName + ".events", EntityType.Topic);
            var topics = namespaces.Select(n => new EntityInfo { Path = topicPath, Type = EntityType.Topic, Namespace = n }).ToArray();

            //TODO: core has a a list of queues as well, which I suppose includes ErrorQ & AuditQ
            // integrate those correctly into the topology
            // settings.Get<QueueBindings>()

            var entities = inputQueues.Concat(topics).ToArray();

            return new TopologySection()
            {
                Namespaces = namespaces,
                Entities = entities
            };
        }

        public TopologySection DeterminePublishDestination(Type eventType)
        {
            var endpointName = settings.Get<EndpointName>();

            var partitioningStrategy = (INamespacePartitioningStrategy)builder.Build(typeof(INamespacePartitioningStrategy));
            var sanitizationStrategy = (ISanitizationStrategy)builder.Build(typeof(ISanitizationStrategy));

            var namespaces = partitioningStrategy.GetNamespaces(endpointName.ToString(), PartitioningIntent.Sending).ToArray();

            var topicPath = sanitizationStrategy.Sanitize(endpointName + ".events", EntityType.Topic);
            var topics = namespaces.Select(n => new EntityInfo { Path = topicPath, Type = EntityType.Topic, Namespace = n }).ToArray();
            
            return new TopologySection
            {
                Namespaces = namespaces,
                Entities = topics
            };
        }

        public TopologySection DetermineSendDestination(string destination)
        {
            var partitioningStrategy = (INamespacePartitioningStrategy)builder.Build(typeof(INamespacePartitioningStrategy));
            var sanitizationStrategy = (ISanitizationStrategy)builder.Build(typeof(ISanitizationStrategy));

            var namespaces = partitioningStrategy.GetNamespaces(destination, PartitioningIntent.Sending).ToArray();

            var inputQueuePath = sanitizationStrategy.Sanitize(destination, EntityType.Queue);
            var inputQueues = namespaces.Select(n => new EntityInfo { Path = inputQueuePath, Type = EntityType.Queue, Namespace = n }).ToArray();

            return new TopologySection
            {
                Namespaces = namespaces,
                Entities = inputQueues
            };
        }

        public TopologySection DetermineResourcesToSubscribeTo(Type eventType)
        {
            if (!subscriptions.ContainsKey(eventType))
            {
                subscriptions[eventType] = BuildSubscriptionHierarchy(eventType);
            }

            return (subscriptions[eventType]);
        }

        TopologySection BuildSubscriptionHierarchy(Type eventType)
        {
            var partitioningStrategy = (INamespacePartitioningStrategy)builder.Build(typeof(INamespacePartitioningStrategy));
            var endpointName = settings.Get<EndpointName>();
            var namespaces = partitioningStrategy.GetNamespaces(endpointName.ToString(), PartitioningIntent.Creating).ToArray();
            var sanitizationStrategy = (ISanitizationStrategy)builder.Build(typeof(ISanitizationStrategy));

            var topicPaths = DetermineTopicsFor(eventType);
            var subscriptionPath = sanitizationStrategy.Sanitize(eventType.FullName, EntityType.Subscription);

            var topics = new List<EntityInfo>();
            var subs = new List<SubscriptionInfo>();
            foreach (var topicPath in topicPaths)
            {
                var path = sanitizationStrategy.Sanitize(topicPath, EntityType.Topic);
                topics.AddRange(namespaces.Select(ns => new EntityInfo()
                {
                    Namespace = ns,
                    Type = EntityType.Topic,
                    Path = path
                }));

                subs.AddRange(namespaces.Select(ns =>
                {
                    var sub = new SubscriptionInfo
                    {
                        Namespace = ns,
                        Type = EntityType.Subscription,
                        Path = subscriptionPath,
                        BrokerSideFilter = new SqlSubscriptionFilter(eventType)
                    };
                    sub.RelationShips.Add(new EntityRelationShipInfo
                    {
                        Source = sub,
                        Target = topics.First(t => t.Namespace == ns),
                        Type = EntityRelationShipType.Subscription
                    });
                    return sub;
                }));
            }
            return new TopologySection
            {
                Entities = subs,
                Namespaces = namespaces
            };
        }

        List<string> DetermineTopicsFor(Type eventType)
        {
            throw new NotImplementedException(); //StaticMessageRouter is obsolete and no public replacement exists at the moment

            //var messageMapper = (IMessageMapper)container.Build(typeof(IMessageMapper));
            //var messageRouter = (StaticMessageRouter)container.Build(typeof(StaticMessageRouter));

            //var destinations = messageRouter.GetDestinationFor(eventType);

            //if (destinations.Any())
            //{
            //    return destinations;
            //}

            //if (messageMapper != null && !eventType.IsInterface)
            //{
            //    var t = messageMapper.GetMappedTypeFor(eventType);
            //    if (t != null && t != eventType)
            //    {
            //        return DetermineTopicsFor(t);
            //    }
            //}

            //return destinations;
        }

        public TopologySection DetermineResourcesToUnsubscribeFrom(Type eventtype)
        {
            TopologySection result;

            if (!subscriptions.TryRemove(eventtype, out result))
            {
                result = new TopologySection
                {
                    Entities = new List<SubscriptionInfo>(),
                    Namespaces = new List<NamespaceInfo>()
                };
            }
           
            return result;
        }
    }
}