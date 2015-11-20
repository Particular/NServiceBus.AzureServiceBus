namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Addressing;
    using Settings;

    public class ForwardingTopologySectionManager : ITopologySectionManager
    {
        SettingsHolder settings;
        ITransportPartsContainer container;

        readonly ConcurrentDictionary<Type, TopologySection> subscriptions = new ConcurrentDictionary<Type, TopologySection>();
        readonly List<EntityInfo> topics = new List<EntityInfo>();

        public ForwardingTopologySectionManager(SettingsHolder settings, ITransportPartsContainer container)
        {
            this.settings = settings;
            this.container = container;
        }

        public TopologySection DetermineReceiveResources()
        {
            return Determine(PartitioningIntent.Receiving);
        }

        public TopologySection DetermineResourcesToCreate()
        {
            return Determine(PartitioningIntent.Creating);
        }

        public TopologySection DeterminePublishDestination(Type eventType)
        {
            var partitioningStrategy = (INamespacePartitioningStrategy)container.Resolve(typeof(INamespacePartitioningStrategy));
            var endpointName = settings.Get<EndpointName>();
            var namespaces = partitioningStrategy.GetNamespaces(endpointName.ToString(), PartitioningIntent.Creating).ToArray();
            var sanitizationStrategy = (ISanitizationStrategy)container.Resolve(typeof(ISanitizationStrategy));
            
            if (!topics.Any())
            {
                BuildTopicBundles(namespaces, sanitizationStrategy);
            }

            return new TopologySection()
            {
                Entities = topics,
                Namespaces = namespaces
            };
        }

        public TopologySection DetermineSendDestination(string destination)
        {
            var partitioningStrategy = (INamespacePartitioningStrategy)container.Resolve(typeof(INamespacePartitioningStrategy));
            var sanitizationStrategy = (ISanitizationStrategy)container.Resolve(typeof(ISanitizationStrategy));

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

        TopologySection BuildSubscriptionHierarchy(Type eventType)
        {
            var partitioningStrategy = (INamespacePartitioningStrategy)container.Resolve(typeof(INamespacePartitioningStrategy));
            var endpointName = settings.Get<EndpointName>();
            var namespaces = partitioningStrategy.GetNamespaces(endpointName.ToString(), PartitioningIntent.Creating).ToArray();
            var sanitizationStrategy = (ISanitizationStrategy)container.Resolve(typeof(ISanitizationStrategy));

            var subscriptionPath = sanitizationStrategy.Sanitize(eventType.FullName, EntityType.Subscription);

            if (!topics.Any())
            {
                BuildTopicBundles(namespaces, sanitizationStrategy);
            }
            var subs = new List<SubscriptionInfo>();
            foreach (var topic in topics)
            {
                subs.AddRange(namespaces.Select(ns =>
                {
                    var inputQueuePath = sanitizationStrategy.Sanitize(endpointName.ToString(), EntityType.Queue);

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
                        Target = topic,
                        Type = EntityRelationShipType.Subscription
                    });
                    sub.RelationShips.Add(new EntityRelationShipInfo
                    {
                        Source = sub,
                        Target = new EntityInfo
                        {
                            Namespace = ns,
                            Path = inputQueuePath,
                            Type = EntityType.Queue
                        },
                        Type = EntityRelationShipType.Forward
                    });
                    return sub;
                }));
            }
            return new TopologySection()
            {
                Entities = subs,
                Namespaces = namespaces
            };
        }
        
        void BuildTopicBundles(NamespaceInfo[] namespaces, ISanitizationStrategy sanitizationStrategy)
        {
            var numberOfEntitiesInBundle = settings.Get<int>(WellKnownConfigurationKeys.Topology.Bundling.NumberOfEntitiesInBundle);
            var bundlePrefix = settings.Get<string>(WellKnownConfigurationKeys.Topology.Bundling.BundlePrefix);

            for (var i = 1; i <= numberOfEntitiesInBundle; i++)
            {
                topics.AddRange(namespaces.Select(n => new EntityInfo
                {
                    Path = sanitizationStrategy.Sanitize(bundlePrefix + i, EntityType.Topic),
                    Type = EntityType.Topic,
                    Namespace = n
                }));
            }
        }
        
        private TopologySection Determine(PartitioningIntent partitioningIntent)
        {
            // computes the topologySectionManager

            var endpointName = settings.Get<EndpointName>();

            var partitioningStrategy = (INamespacePartitioningStrategy)container.Resolve(typeof(INamespacePartitioningStrategy));
            var sanitizationStrategy = (ISanitizationStrategy)container.Resolve(typeof(ISanitizationStrategy));

            var namespaces = partitioningStrategy.GetNamespaces(endpointName.ToString(), partitioningIntent).ToArray();

            var inputQueuePath = sanitizationStrategy.Sanitize(endpointName.ToString(), EntityType.Queue);
            var inputQueues = namespaces.Select(n => new EntityInfo { Path = inputQueuePath, Type = EntityType.Queue, Namespace = n }).ToArray();

            if (!topics.Any())
            {
                BuildTopicBundles(namespaces, sanitizationStrategy);
            }

            //TODO: core has a a list of queues as well, which I suppose includes ErrorQ & AuditQ
            // integrate those correctly into the topologySectionManager
            // settings.Get<QueueBindings>()

            var entities = inputQueues.Concat(topics).ToArray();

            return new TopologySection
            {
                Namespaces = namespaces,
                Entities = entities
            };
        }
    }
}