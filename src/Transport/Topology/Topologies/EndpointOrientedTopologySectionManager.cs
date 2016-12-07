namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using Transport;
    using Settings;

    class EndpointOrientedTopologySectionManager : ITopologySectionManagerInternal
    {
        SettingsHolder settings;
        ITransportPartsContainer container;
        ConcurrentDictionary<Type, TopologySectionInternal> subscriptions = new ConcurrentDictionary<Type, TopologySectionInternal>();

        public EndpointOrientedTopologySectionManager(SettingsHolder settings, ITransportPartsContainer container)
        {
            this.settings = settings;
            this.container = container;
        }

        public TopologySectionInternal DetermineReceiveResources(string inputQueue)
        {
            var partitioningStrategy = (INamespacePartitioningStrategy)container.Resolve(typeof(INamespacePartitioningStrategy));
            var addressingLogic = (AddressingLogic)container.Resolve(typeof(AddressingLogic));

            var namespaces = partitioningStrategy.GetNamespaces(PartitioningIntent.Receiving).ToArray();

            var inputQueuePath = addressingLogic.Apply(inputQueue, EntityType.Queue).Name;
            var entities = namespaces.Select(n => new EntityInfoInternal { Path = inputQueuePath, Type = EntityType.Queue, Namespace = n }).ToList();

            return new TopologySectionInternal()
            {
                Namespaces = namespaces,
                Entities = entities.ToArray()
            };
        }

        public TopologySectionInternal DetermineResourcesToCreate(QueueBindings queueBindings)
        {
            // computes the topologySectionManager

            var endpointName = settings.EndpointName();

            var partitioningStrategy = (INamespacePartitioningStrategy)container.Resolve(typeof(INamespacePartitioningStrategy));
            var addressingLogic = (AddressingLogic)container.Resolve(typeof(AddressingLogic));

            var namespaces = partitioningStrategy.GetNamespaces(PartitioningIntent.Creating).ToArray();

            var inputQueuePath = addressingLogic.Apply(endpointName, EntityType.Queue).Name;
            var entities = namespaces.Select(n => new EntityInfoInternal { Path = inputQueuePath, Type = EntityType.Queue, Namespace = n }).ToList();

            var topicPath = addressingLogic.Apply(endpointName + ".events", EntityType.Topic).Name;
            var topics = namespaces.Select(n => new EntityInfoInternal { Path = topicPath, Type = EntityType.Topic, Namespace = n}).ToArray();
            entities.AddRange(topics);

            foreach (var n in namespaces)
            {
                entities.AddRange(queueBindings.ReceivingAddresses.Select(p => new EntityInfoInternal
                {
                    Path = addressingLogic.Apply(p, EntityType.Queue).Name,
                    Type = EntityType.Queue,
                    Namespace = n
                }));

                // assumed errorq and auditq are in here
                entities.AddRange(queueBindings.SendingAddresses.Select(p => new EntityInfoInternal
                {
                    Path = addressingLogic.Apply(p, EntityType.Queue).Name,
                    Type = EntityType.Queue,
                    Namespace = n
                }));
            }


            return new TopologySectionInternal
            {
                Namespaces = namespaces,
                Entities = entities.ToArray()
            };
        }

        public TopologySectionInternal DeterminePublishDestination(Type eventType)
        {
            var endpointName = settings.EndpointName();

            var partitioningStrategy = (INamespacePartitioningStrategy)container.Resolve(typeof(INamespacePartitioningStrategy));
            var addressingLogic = (AddressingLogic)container.Resolve(typeof(AddressingLogic));

            var namespaces = partitioningStrategy.GetNamespaces(PartitioningIntent.Sending).Where(n => n.Mode == NamespaceMode.Active).ToArray();

            var topicPath = addressingLogic.Apply(endpointName + ".events", EntityType.Topic).Name;
            var topics = namespaces.Select(n => new EntityInfoInternal { Path = topicPath, Type = EntityType.Topic, Namespace = n }).ToArray();

            return new TopologySectionInternal
            {
                Namespaces = namespaces,
                Entities = topics
            };
        }

        public TopologySectionInternal DetermineSendDestination(string destination)
        {
            var partitioningStrategy = (INamespacePartitioningStrategy)container.Resolve(typeof(INamespacePartitioningStrategy));
            var addressingLogic = (AddressingLogic)container.Resolve(typeof(AddressingLogic));
            var defaultName = settings.Get<string>(WellKnownConfigurationKeys.Topology.Addressing.DefaultNamespaceAlias);

            var inputQueueAddress = addressingLogic.Apply(destination, EntityType.Queue);

            RuntimeNamespaceInfo[] namespaces = null;
            if (inputQueueAddress.HasSuffix && inputQueueAddress.Suffix != defaultName) // sending to specific namespace
            {
                if (inputQueueAddress.HasConnectionString)
                {
                    namespaces = new[]{
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

            var inputQueues = namespaces.Select(n => new EntityInfoInternal { Path = inputQueueAddress.Name, Type = EntityType.Queue, Namespace = n }).ToArray();

            return new TopologySectionInternal
            {
                Namespaces = namespaces,
                Entities = inputQueues
            };
        }

        public TopologySectionInternal DetermineResourcesToSubscribeTo(Type eventType)
        {
            if (!subscriptions.ContainsKey(eventType))
            {
                subscriptions[eventType] = BuildSubscriptionHierarchy(eventType);
            }

            return (subscriptions[eventType]);
        }

        TopologySectionInternal BuildSubscriptionHierarchy(Type eventType)
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

            var topics = new List<EntityInfoInternal>();
            var subs = new List<SubscriptionInfoInternal>();
            foreach (var topicPath in topicPaths)
            {
                var path = addressingLogic.Apply(topicPath, EntityType.Topic).Name;
                topics.AddRange(namespaces.Select(ns => new EntityInfoInternal()
                {
                    Namespace = ns,
                    Type = EntityType.Topic,
                    Path = path,
                }));

                subs.AddRange(namespaces.Select(ns =>
                {
                    var sub = new SubscriptionInfoInternal
                    {
                        Namespace = ns,
                        Type = EntityType.Subscription,
                        Path = subscriptionNameV6,
                        Metadata = new SubscriptionMetadataInternal
                        {
                            Description = endpointName + " subscribed to " + eventType.FullName,
                            SubscriptionNameBasedOnEventWithNamespace = subscriptionName
                        },
                        BrokerSideFilter = new SqlSubscriptionFilter(eventType),
                        ShouldBeListenedTo = true
                    };
                    sub.RelationShips.Add(new EntityRelationShipInfoInternal
                    {
                        Source = sub,
                        Target = topics.First(t => t.Namespace == ns),
                        Type = EntityRelationShipTypeInternal.Subscription
                    });
                    return sub;
                }));
            }
            return new TopologySectionInternal
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

        public TopologySectionInternal DetermineResourcesToUnsubscribeFrom(Type eventtype)
        {
            TopologySectionInternal result;

            if (!subscriptions.TryRemove(eventtype, out result))
            {
                result = new TopologySectionInternal
                {
                    Entities = new List<SubscriptionInfoInternal>(),
                    Namespaces = new List<RuntimeNamespaceInfo>()
                };
            }

            return result;
        }
    }
}