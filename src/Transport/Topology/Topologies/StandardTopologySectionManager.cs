namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Addressing;
    using NServiceBus.Config;
    using NServiceBus.Transports;
    using Settings;

    public class StandardTopologySectionManager : ITopologySectionManager
    {
        SettingsHolder settings;
        ITransportPartsContainer container;

        readonly ConcurrentDictionary<Type, TopologySection> subscriptions = new ConcurrentDictionary<Type, TopologySection>();

        public StandardTopologySectionManager(SettingsHolder settings, ITransportPartsContainer container)
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

        private TopologySection Determine(PartitioningIntent partitioningIntent)
        {
            // computes the topologySectionManager

            var endpointName = settings.EndpointName();

            var partitioningStrategy = (INamespacePartitioningStrategy)container.Resolve(typeof(INamespacePartitioningStrategy));
            var sanitizationStrategy = (ISanitizationStrategy)container.Resolve(typeof(ISanitizationStrategy));
            
            var namespaces = partitioningStrategy.GetNamespaces(endpointName.ToString(), partitioningIntent).ToArray();

            var inputQueuePath = sanitizationStrategy.Sanitize(endpointName.ToString(), EntityType.Queue);
            var entities = namespaces.Select(n => new EntityInfo { Path = inputQueuePath, Type = EntityType.Queue, Namespace = n }).ToList();

            if (partitioningIntent == PartitioningIntent.Creating)
            {
                var topicPath = sanitizationStrategy.Sanitize(endpointName + ".events", EntityType.Topic);
                var topics =
                    namespaces.Select(n => new EntityInfo {Path = topicPath, Type = EntityType.Topic, Namespace = n})
                        .ToArray();
                entities.AddRange(topics);
            }

            if (settings.HasExplicitValue<QueueBindings>())
            {
                var queueBindings = settings.Get<QueueBindings>();
                foreach (var n in namespaces)
                {
                    entities.AddRange(queueBindings.ReceivingAddresses.Select(p => new EntityInfo
                    {
                        Path = p,
                        Type = EntityType.Queue,
                        Namespace = n
                    }));

                    if (partitioningIntent == PartitioningIntent.Creating)
                    {
                        // assumed errorq and auditq are in here
                        entities.AddRange(queueBindings.SendingAddresses.Select(p => new EntityInfo
                        {
                            Path = p,
                            Type = EntityType.Queue,
                            Namespace = n
                        }));
                    }
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
            var sanitizationStrategy = (ISanitizationStrategy)container.Resolve(typeof(ISanitizationStrategy));

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

        TopologySection BuildSubscriptionHierarchy(Type eventType)
        {
            var partitioningStrategy = (INamespacePartitioningStrategy) container.Resolve(typeof(INamespacePartitioningStrategy));
            var endpointName = settings.EndpointName();
            var namespaces = partitioningStrategy.GetNamespaces(endpointName.ToString(), PartitioningIntent.Creating).ToArray();
            var sanitizationStrategy = (ISanitizationStrategy) container.Resolve(typeof(ISanitizationStrategy));

            var topicPaths = DetermineTopicsFor(eventType);
            // TODO: issue
            // subscriptions in V6 were done on type.Name or type.FullName, as here only on FullName
            var subscriptionNameCandidate = endpointName + "." + eventType.FullName;
            var subscriptionPath = sanitizationStrategy.Sanitize(subscriptionNameCandidate, EntityType.Subscription);

            var topics = new List<EntityInfo>();
            var subs = new List<SubscriptionInfo>();
            foreach (var topicPath in topicPaths)
            {
                var path = sanitizationStrategy.Sanitize(topicPath, EntityType.Topic);
                topics.AddRange(namespaces.Select(ns => new EntityInfo()
                {
                    Namespace = ns,
                    Type = EntityType.Topic,
                    Path = path,
                    Metadata = topicPath
                }));

                subs.AddRange(namespaces.Select(ns =>
                {
                    var sub = new SubscriptionInfo
                    {
                        Namespace = ns,
                        Type = EntityType.Subscription,
                        Path = subscriptionPath,
                        Metadata = endpointName + " subscribed to " + eventType.FullName,
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
            var result = new List<string>();
            var unicastBusConfig = settings.GetConfigSection<UnicastBusConfig>();
            if (unicastBusConfig != null)
            {
                var legacyRoutingConfig = unicastBusConfig.MessageEndpointMappings;

                foreach (MessageEndpointMapping m in legacyRoutingConfig)
                {
                    m.Configure((type, address) =>
                    {
                        if(type == eventType)
                        {
                            var path = address + ".events";
                            if(!result.Contains(path)) result.Add(path);
                        }
                    });
                }
            }
            return result;
            //var messageMapper = (IMessageMapper)container.Build(typeof());
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