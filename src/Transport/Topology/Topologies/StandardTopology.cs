namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Addressing;
    using ObjectBuilder.Common;
    using Settings;

    public class StandardTopology : ITopology
    {
        readonly SettingsHolder settings;
        readonly IContainer container;

        readonly ConcurrentDictionary<Type, List<SubscriptionInfo>> subscriptions = new ConcurrentDictionary<Type, List<SubscriptionInfo>>();

        public StandardTopology(SettingsHolder settings, IContainer container)
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
            // configures container
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

        public TopologyDefinition Determine(Purpose purpose)
        {
            // computes the topology

            var endpointName = settings.Get<EndpointName>();

            var partitioningStrategy = (INamespacePartitioningStrategy)container.Build(typeof(INamespacePartitioningStrategy));
            var sanitizationStrategy = (ISanitizationStrategy)container.Build(typeof(ISanitizationStrategy));
            
            var namespaces = partitioningStrategy.GetNamespaces(endpointName.ToString(), purpose).ToArray();

            var inputQueuePath = sanitizationStrategy.Sanitize(endpointName.ToString(), EntityType.Queue);
            var inputQueues = namespaces.Select(n => new EntityInfo { Path = inputQueuePath, Type = EntityType.Queue, Namespace = n }).ToArray();

            var topicPath = sanitizationStrategy.Sanitize(endpointName + ".events", EntityType.Topic);
            var topics = namespaces.Select(n => new EntityInfo { Path = topicPath, Type = EntityType.Topic, Namespace = n }).ToArray();

            var entities = inputQueues.Concat(topics).ToArray();

            return new TopologyDefinition()
            {
                Namespaces = namespaces,
                Entities = entities
            };
        }

        public IEnumerable<SubscriptionInfo> Subscribe(Type eventType)
        {
            if (!subscriptions.ContainsKey(eventType))
            {
                subscriptions[eventType] = BuildSubscriptionHierarchy(eventType);
            }

            return (subscriptions[eventType]);
        }

        List<SubscriptionInfo> BuildSubscriptionHierarchy(Type eventType)
        {
            var partitioningStrategy = (INamespacePartitioningStrategy) container.Build(typeof(INamespacePartitioningStrategy));
            var endpointName = settings.Get<EndpointName>();
            var namespaces = partitioningStrategy.GetNamespaces(endpointName.ToString(), Purpose.Creating).ToArray();
            var sanitizationStrategy = (ISanitizationStrategy) container.Build(typeof(ISanitizationStrategy));

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
            return subs;
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

        public IEnumerable<SubscriptionInfo> Unsubscribe(Type eventtype)
        {
            List<SubscriptionInfo> result;

            if (!subscriptions.TryRemove(eventtype, out result))
            {
                result = new List<SubscriptionInfo>();
            }
           
            return result;
        }
    }
}