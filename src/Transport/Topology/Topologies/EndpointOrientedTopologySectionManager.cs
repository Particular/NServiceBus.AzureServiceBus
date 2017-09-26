namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    class EndpointOrientedTopologySectionManager : ITopologySectionManagerInternal
    {
        public EndpointOrientedTopologySectionManager(string defaultNameSpaceAlias, NamespaceConfigurations namespaceConfigurations, string endpointName, PublishersConfiguration publishersConfiguration, INamespacePartitioningStrategy namespacePartitioningStrategy, AddressingLogic addressingLogic)
        {
            this.namespaceConfigurations = namespaceConfigurations;
            this.defaultNameSpaceAlias = defaultNameSpaceAlias;
            this.endpointName = endpointName;
            this.addressingLogic = addressingLogic;
            this.namespacePartitioningStrategy = namespacePartitioningStrategy;
            this.publishersConfiguration = publishersConfiguration;
        }

        public TopologySectionInternal DetermineReceiveResources(string inputQueue)
        {
            var namespaces = namespacePartitioningStrategy.GetNamespaces(PartitioningIntent.Receiving).ToArray();

            var inputQueuePath = addressingLogic.Apply(inputQueue, EntityType.Queue).Name;
            var entities = namespaces.Select(n => new EntityInfoInternal
            {
                Path = inputQueuePath,
                Type = EntityType.Queue,
                Namespace = n
            }).ToList();

            return new TopologySectionInternal
            {
                Namespaces = namespaces,
                Entities = entities.ToArray()
            };
        }

        public TopologySectionInternal DetermineResourcesToCreate(QueueBindings queueBindings)
        {
            // computes the topologySectionManager

            var namespaces = namespacePartitioningStrategy.GetNamespaces(PartitioningIntent.Creating).ToArray();

            var inputQueuePath = addressingLogic.Apply(endpointName, EntityType.Queue).Name;
            var entities = namespaces.Select(n => new EntityInfoInternal
            {
                Path = inputQueuePath,
                Type = EntityType.Queue,
                Namespace = n
            }).ToList();

            var topicPath = addressingLogic.Apply(endpointName + ".events", EntityType.Topic).Name;
            var topics = namespaces.Select(n => new EntityInfoInternal
            {
                Path = topicPath,
                Type = EntityType.Topic,
                Namespace = n
            }).ToArray();
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
            var namespaces = namespacePartitioningStrategy.GetNamespaces(PartitioningIntent.Sending).Where(n => n.Mode == NamespaceMode.Active).ToArray();

            var topicPath = addressingLogic.Apply(endpointName + ".events", EntityType.Topic).Name;
            var topics = namespaces.Select(n => new EntityInfoInternal
            {
                Path = topicPath,
                Type = EntityType.Topic,
                Namespace = n
            }).ToArray();

            return new TopologySectionInternal
            {
                Namespaces = namespaces,
                Entities = topics
            };
        }

        public TopologySectionInternal DetermineSendDestination(string destination)
        {
            var inputQueueAddress = addressingLogic.Apply(destination, EntityType.Queue);

            RuntimeNamespaceInfo[] namespaces = null;
            if (inputQueueAddress.HasSuffix && inputQueueAddress.Suffix != defaultNameSpaceAlias) // sending to specific namespace
            {
                if (inputQueueAddress.HasConnectionString)
                {
                    namespaces = new[]
                    {
                        new RuntimeNamespaceInfo(inputQueueAddress.Suffix, inputQueueAddress.Suffix, NamespacePurpose.Routing)
                    };
                }
                else
                {
                    var configured = namespaceConfigurations.FirstOrDefault(n => n.Alias == inputQueueAddress.Suffix);
                    if (configured != null)
                    {
                        namespaces = new[]
                        {
                            new RuntimeNamespaceInfo(configured.Alias, configured.Connection, configured.Purpose)
                        };
                    }
                }
            }
            else
            {
                var configured = namespaceConfigurations.FirstOrDefault(n => n.RegisteredEndpoints.Contains(destination, StringComparer.OrdinalIgnoreCase));
                if (configured != null)
                {
                    namespaces = new[]
                    {
                        new RuntimeNamespaceInfo(configured.Alias, configured.Connection, configured.Purpose),
                    };
                }
                else // sending to the partition
                {
                    namespaces = namespacePartitioningStrategy.GetNamespaces(PartitioningIntent.Sending).ToArray();
                }
            }

            if (namespaces == null)
            {
                throw new Exception($"Could not determine namespace for destination {destination}");
            }

            var inputQueues = namespaces.Select(n => new EntityInfoInternal
            {
                Path = inputQueueAddress.Name,
                Type = EntityType.Queue,
                Namespace = n
            }).ToArray();

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

            return subscriptions[eventType];
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

        TopologySectionInternal BuildSubscriptionHierarchy(Type eventType)
        {
            var namespaces = namespacePartitioningStrategy.GetNamespaces(PartitioningIntent.Creating).ToArray();

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
                topics.AddRange(namespaces.Select(ns => new EntityInfoInternal
                {
                    Namespace = ns,
                    Type = EntityType.Topic,
                    Path = path
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
                        Target = topics.First(t => t.Path == path && t.Namespace == ns),
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
            return publishersConfiguration
                .GetPublishersFor(eventType)
                .Select(x => string.Concat(x, ".events"))
                .ToList();
        }

        ConcurrentDictionary<Type, TopologySectionInternal> subscriptions = new ConcurrentDictionary<Type, TopologySectionInternal>();
        INamespacePartitioningStrategy namespacePartitioningStrategy;
        AddressingLogic addressingLogic;
        PublishersConfiguration publishersConfiguration;
        string endpointName;
        string defaultNameSpaceAlias;
        NamespaceConfigurations namespaceConfigurations;
    }
}