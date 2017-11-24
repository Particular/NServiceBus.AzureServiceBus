namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Transport;
    using Settings;

    class EndpointOrientedTopologySectionManager : ITopologySectionManager
    {
        SettingsHolder settings;
        ITransportPartsContainer container;
        ConcurrentDictionary<Type, TopologySection> subscriptions = new ConcurrentDictionary<Type, TopologySection>();

        ConcurrentDictionary<string, TopologySection> sendDestinations = new ConcurrentDictionary<string, TopologySection>();
        ConcurrentDictionary<Type, TopologySection> publishDestinations = new ConcurrentDictionary<Type, TopologySection>();

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

            var inputQueuePath = addressingLogic.Apply(inputQueue, EntityType.Queue).Name;
            var entities = namespaces.Select(n => new EntityInfo { Path = inputQueuePath, Type = EntityType.Queue, Namespace = n }).ToList();

            return new TopologySection()
            {
                Namespaces = namespaces,
                Entities = entities.ToArray()
            };
        }

        public TopologySection DetermineResourcesToCreate(QueueBindings queueBindings)
        {
            // computes the topologySectionManager

            var endpointName = settings.EndpointName();

            var partitioningStrategy = (INamespacePartitioningStrategy)container.Resolve(typeof(INamespacePartitioningStrategy));
            var addressingLogic = (AddressingLogic)container.Resolve(typeof(AddressingLogic));

            var namespaces = partitioningStrategy.GetNamespaces(PartitioningIntent.Creating).ToArray();

            var inputQueuePath = addressingLogic.Apply(endpointName, EntityType.Queue).Name;
            var entities = namespaces.Select(n => new EntityInfo { Path = inputQueuePath, Type = EntityType.Queue, Namespace = n }).ToList();

            var topicPath = addressingLogic.Apply($"{endpointName}.events", EntityType.Topic).Name;
            var topics = namespaces.Select(n => new EntityInfo {Path = topicPath, Type = EntityType.Topic, Namespace = n}).ToArray();
            entities.AddRange(topics);

            foreach (var n in namespaces)
            {
                entities.AddRange(queueBindings.ReceivingAddresses.Select(p => new EntityInfo
                {
                    Path = addressingLogic.Apply(p, EntityType.Queue).Name,
                    Type = EntityType.Queue,
                    Namespace = n
                }));

                // assumed errorq and auditq are in here
                entities.AddRange(queueBindings.SendingAddresses.Select(p => new EntityInfo
                {
                    Path = addressingLogic.Apply(p, EntityType.Queue).Name,
                    Type = EntityType.Queue,
                    Namespace = n
                }));
            }


            return new TopologySection
            {
                Namespaces = namespaces,
                Entities = entities.ToArray()
            };
        }

        public TopologySection DeterminePublishDestination(Type eventType)
        {
            var partitioningStrategy = (INamespacePartitioningStrategy)container.Resolve(typeof(INamespacePartitioningStrategy));
            var canCache = (partitioningStrategy as ICacheableNamespacePartitioningStrategy)?.SendingNamespacesCanBeCached ?? true;

            return canCache ? publishDestinations.GetOrAdd(eventType, t => CreateSectionForPublish(partitioningStrategy)) : CreateSectionForPublish(partitioningStrategy);
        }

        TopologySection CreateSectionForPublish(INamespacePartitioningStrategy partitioningStrategy)
        {
            var endpointName = settings.EndpointName();

            var addressingLogic = (AddressingLogic) container.Resolve(typeof(AddressingLogic));

            var namespaces = partitioningStrategy.GetNamespaces(PartitioningIntent.Sending).ToArray();

            var topicPath = addressingLogic.Apply($"{endpointName}.events", EntityType.Topic).Name;
            var topics = namespaces.Select(n => new EntityInfo
            {
                Path = topicPath,
                Type = EntityType.Topic,
                Namespace = n
            }).ToArray();

            return new TopologySection
            {
                Namespaces = namespaces,
                Entities = topics
            };
        }

        public TopologySection DetermineSendDestination(string destination)
        {
            var partitioningStrategy = (INamespacePartitioningStrategy)container.Resolve(typeof(INamespacePartitioningStrategy));
            var canCache = (partitioningStrategy as ICacheableNamespacePartitioningStrategy)?.SendingNamespacesCanBeCached ?? true;

            return canCache ? sendDestinations.GetOrAdd(destination, dest => CreateSectionForSend(dest)) : CreateSectionForSend(destination);
        }

        TopologySection CreateSectionForSend(string destination)
        {
            var partitioningStrategy = (INamespacePartitioningStrategy) container.Resolve(typeof(INamespacePartitioningStrategy));
            var addressingLogic = (AddressingLogic) container.Resolve(typeof(AddressingLogic));
            var defaultName = settings.Get<string>(WellKnownConfigurationKeys.Topology.Addressing.DefaultNamespaceAlias);

            var inputQueueAddress = addressingLogic.Apply(destination, EntityType.Queue);

            RuntimeNamespaceInfo[] namespaces = null;
            if (inputQueueAddress.HasSuffix && inputQueueAddress.Suffix != defaultName) // sending to specific namespace
            {
                if (inputQueueAddress.HasConnectionString)
                {
                    namespaces = new[]
                    {
                        new RuntimeNamespaceInfo(inputQueueAddress.Suffix, inputQueueAddress.Suffix, NamespacePurpose.Routing, NamespaceMode.Active)
                    };
                }
                else
                {
                    NamespaceConfigurations configuredNamespaces;
                    if (settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, out configuredNamespaces))
                    {
                        var configured = configuredNamespaces.FirstOrDefault(n => n.Alias == inputQueueAddress.Suffix);
                        if (configured != null)
                        {
                            namespaces = new[]
                            {
                                new RuntimeNamespaceInfo(configured.Alias, configured.ConnectionString, configured.Purpose, NamespaceMode.Active)
                            };
                        }
                    }
                }
            }
            else // sending to the partition
            {
                namespaces = partitioningStrategy.GetNamespaces(PartitioningIntent.Sending).ToArray();
            }

            if (namespaces == null)
            {
                throw new Exception($"Could not determine namespace for destination {destination}");
            }

            var inputQueues = namespaces.Select(n => new EntityInfo
            {
                Path = inputQueueAddress.Name,
                Type = EntityType.Queue,
                Namespace = n
            }).ToArray();

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
            var subscriptionNameV6 = addressingLogic.Apply(subscriptionNameCandidateV6, EntityType.Subscription).Name;
            var subscriptionNameCandidate = endpointName + "." + eventType.FullName;
            var subscriptionName = addressingLogic.Apply(subscriptionNameCandidate, EntityType.Subscription).Name;

            var topics = new List<EntityInfo>();
            var subs = new List<SubscriptionInfo>();
            foreach (var topicPath in topicPaths)
            {
                var path = addressingLogic.Apply(topicPath, EntityType.Topic).Name;
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
                        Target = topics.First(t => t.Path == path && t.Namespace == ns),
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