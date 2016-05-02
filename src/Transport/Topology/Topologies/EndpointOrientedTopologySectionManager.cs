namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Addressing;
    using Transports;
    using Settings;

    class EndpointOrientedTopologySectionManager : ITopologySectionManager
    {
        SettingsHolder settings;
        ITransportPartsContainer container;
        ConcurrentDictionary<Type, TopologySection> subscriptions = new ConcurrentDictionary<Type, TopologySection>();

        public EndpointOrientedTopologySectionManager(SettingsHolder settings, ITransportPartsContainer container)
        {
            this.settings = settings;
            this.container = container;
        }

        public TopologySection DetermineReceiveResources(string inputQueue)
        {
            var partitioningStrategy = (INamespacePartitioningStrategy)container.Resolve(typeof(INamespacePartitioningStrategy));
            var addressingLogic = (AddressingLogic)container.Resolve(typeof(AddressingLogic));

            var namespaces = partitioningStrategy.GetNamespaces(PartitioningIntent.Receiving).ToArray();

            var inputQueuePath = addressingLogic.Apply(inputQueue, EntityType.Queue);
            var entities = namespaces.Select(n => new EntityInfo { Path = inputQueuePath, Type = EntityType.Queue, Namespace = n }).ToList();

            return new TopologySection()
            {
                Namespaces = namespaces,
                Entities = entities.ToArray()
            };
        }

        public TopologySection DetermineResourcesToCreate()
        {
            // computes the topologySectionManager

            var endpointName = settings.EndpointName();

            var partitioningStrategy = (INamespacePartitioningStrategy)container.Resolve(typeof(INamespacePartitioningStrategy));
            var addressingLogic = (AddressingLogic)container.Resolve(typeof(AddressingLogic));

            var namespaces = partitioningStrategy.GetNamespaces(PartitioningIntent.Creating).ToArray();

            var inputQueuePath = addressingLogic.Apply(endpointName.ToString(), EntityType.Queue);
            var entities = namespaces.Select(n => new EntityInfo { Path = inputQueuePath, Type = EntityType.Queue, Namespace = n }).ToList();

            var topicPath = addressingLogic.Apply(endpointName + ".events", EntityType.Topic);
            var topics =
                namespaces.Select(n => new EntityInfo {Path = topicPath, Type = EntityType.Topic, Namespace = n})
                    .ToArray();
            entities.AddRange(topics);

            if (settings.HasExplicitValue<QueueBindings>())
            {
                var queueBindings = settings.Get<QueueBindings>();
                foreach (var n in namespaces)
                {
                    entities.AddRange(queueBindings.ReceivingAddresses.Select(p => new EntityInfo
                    {
                        Path = sanitizationStrategy.Sanitize(p, EntityType.Queue),
                        Type = EntityType.Queue,
                        Namespace = n
                    }));

                    // assumed errorq and auditq are in here
                    entities.AddRange(queueBindings.SendingAddresses.Select(p => new EntityInfo
                    {
                        Path = sanitizationStrategy.Sanitize(p, EntityType.Queue),
                        Type = EntityType.Queue,
                        Namespace = n
                    }));
                }
            }

            return new TopologySection()
            {
                Namespaces = namespaces,
                Entities = entities.ToArray()
            };
        }

        public TopologySection DeterminePublishDestination(Type eventType)
        {
            var endpointName = settings.EndpointName();

            var partitioningStrategy = (INamespacePartitioningStrategy)container.Resolve(typeof(INamespacePartitioningStrategy));
            var addressingLogic = (AddressingLogic)container.Resolve(typeof(AddressingLogic));

            var namespaces = partitioningStrategy.GetNamespaces(PartitioningIntent.Sending).Where(n => n.Mode == NamespaceMode.Active).ToArray();

            var topicPath = addressingLogic.Apply(endpointName + ".events", EntityType.Topic);
            var topics = namespaces.Select(n => new EntityInfo { Path = topicPath, Type = EntityType.Topic, Namespace = n }).ToArray();

            return new TopologySection
            {
                Namespaces = namespaces,
                Entities = topics
            };
        }

        public TopologySection DetermineSendDestination(string destination)
        {
            var partitioningStrategy = (INamespacePartitioningStrategy)container.Resolve(typeof(INamespacePartitioningStrategy));
            var addressingLogic = (AddressingLogic)container.Resolve(typeof(AddressingLogic));

            var namespaces = partitioningStrategy.GetNamespaces(PartitioningIntent.Sending).Where(n => n.Mode == NamespaceMode.Active).ToArray();

            var inputQueuePath = addressingLogic.Apply(destination, EntityType.Queue);
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
            var partitioningStrategy = (INamespacePartitioningStrategy) container.Resolve(typeof(INamespacePartitioningStrategy));
            var endpointName = settings.EndpointName();
            var namespaces = partitioningStrategy.GetNamespaces(PartitioningIntent.Creating).ToArray();
            var addressingLogic = (AddressingLogic) container.Resolve(typeof(AddressingLogic));

            var topicPaths = DetermineTopicsFor(eventType);

            var subscriptionNameCandidateV6 = endpointName + "." + eventType.Name;
            var subscriptionNameV6 = addressingLogic.Apply(subscriptionNameCandidateV6, EntityType.Subscription);
            var subscriptionNameCandidate = endpointName + "." + eventType.FullName;
            var subscriptionName = addressingLogic.Apply(subscriptionNameCandidate, EntityType.Subscription);

            var topics = new List<EntityInfo>();
            var subs = new List<SubscriptionInfo>();
            foreach (var topicPath in topicPaths)
            {
                var path = addressingLogic.Apply(topicPath, EntityType.Topic);
                topics.AddRange(namespaces.Select(ns => new EntityInfo()
                {
                    Namespace = ns,
                    Type = EntityType.Topic,
                    Path = path,
                }));

                subs.AddRange(namespaces.Select(ns =>
                {
                    var sub = new SubscriptionInfo
                    {
                        Namespace = ns,
                        Type = EntityType.Subscription,
                        Path = subscriptionNameV6,
                        Metadata = new SubscriptionMetadata
                        {
                            Description = endpointName + " subscribed to " + eventType.FullName,
                            SubscriptionNameBasedOnEventWithNamespace = subscriptionName
                        },
                        BrokerSideFilter = new SqlSubscriptionFilter(eventType),
                        ShouldBeListenedTo = true
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
            var configuration = container.Resolve<PublishersConfiguration>();
            return configuration
                .GetPublishersFor(eventType)
                .Select(x => string.Concat(x, ".events"))
                .ToList();
        }

        public TopologySection DetermineResourcesToUnsubscribeFrom(Type eventtype)
        {
            TopologySection result;

            if (!subscriptions.TryRemove(eventtype, out result))
            {
                result = new TopologySection
                {
                    Entities = new List<SubscriptionInfo>(),
                    Namespaces = new List<RuntimeNamespaceInfo>()
                };
            }

            return result;
        }
    }
}