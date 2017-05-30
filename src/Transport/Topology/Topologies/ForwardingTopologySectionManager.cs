namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using NServiceBus.AzureServiceBus.Topology.MetaModel;
    using Transport;
    using Settings;

    class ForwardingTopologySectionManager : ITopologySectionManager
    {
        SettingsHolder settings;
        ITransportPartsContainer container;

        readonly ConcurrentDictionary<Type, TopologySection> subscriptions = new ConcurrentDictionary<Type, TopologySection>();
        readonly ConcurrentDictionary<string, TopologySection> sendDestinations = new ConcurrentDictionary<string, TopologySection>();
        readonly ConcurrentDictionary<Type, TopologySection> publishDestinations = new ConcurrentDictionary<Type, TopologySection>();
        readonly List<EntityInfo> topics = new List<EntityInfo>();
        Lazy<NamespaceBundleConfigurations> namespaceBundleConfigurations;

        public ForwardingTopologySectionManager(SettingsHolder settings, ITransportPartsContainer container)
        {
            this.settings = settings;
            this.container = container;

            namespaceBundleConfigurations = new Lazy<NamespaceBundleConfigurations>(() =>
            {
                var manageNamespaceManagerLifeCycle = container.Resolve<IManageNamespaceManagerLifeCycle>();
                var namespaceConfigurations = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
                var bundlePrefix = settings.Get<string>(WellKnownConfigurationKeys.Topology.Bundling.BundlePrefix);
                var bundleConfigurations = NumberOfTopicsInBundleCheck.Run(manageNamespaceManagerLifeCycle, namespaceConfigurations, bundlePrefix).GetAwaiter().GetResult();
                return bundleConfigurations;
            });
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
            var endpointName = settings.EndpointName();

            var partitioningStrategy = (INamespacePartitioningStrategy)container.Resolve(typeof(INamespacePartitioningStrategy));
            var addressingLogic = (AddressingLogic)container.Resolve(typeof(AddressingLogic));

            var namespaces = partitioningStrategy.GetNamespaces(PartitioningIntent.Creating).ToArray();

            var inputQueuePath = addressingLogic.Apply(endpointName, EntityType.Queue).Name;
            var inputQueues = namespaces.Select(n => new EntityInfo
            {
                Path = inputQueuePath,
                Type = EntityType.Queue,
                Namespace = n
            }).ToList();

            if (!topics.Any())
            {
                BuildTopicBundles(namespaces, addressingLogic);
            }

            foreach (var n in namespaces)
            {
                inputQueues.AddRange(queueBindings.ReceivingAddresses.Select(p => new EntityInfo
                {
                    Path = addressingLogic.Apply(p, EntityType.Queue).Name,
                    Type = EntityType.Queue,
                    Namespace = n
                }));

                inputQueues.AddRange(queueBindings.SendingAddresses.Select(p => new EntityInfo
                {
                    Path = addressingLogic.Apply(p, EntityType.Queue).Name,
                    Type = EntityType.Queue,
                    Namespace = n
                }));
            }

            var entities = inputQueues.Concat(topics).ToArray();

            return new TopologySection
            {
                Namespaces = namespaces,
                Entities = entities
            };
        }

        public TopologySection DeterminePublishDestination(Type eventType)
        {
            return publishDestinations.GetOrAdd(eventType, t =>
            {
                var partitioningStrategy = (INamespacePartitioningStrategy)container.Resolve(typeof(INamespacePartitioningStrategy));
                var namespaces = partitioningStrategy.GetNamespaces(PartitioningIntent.Sending).Where(n => n.Mode == NamespaceMode.Active).ToArray();
                var addressingLogic = (AddressingLogic)container.Resolve(typeof(AddressingLogic));

                if (!topics.Any())
                {
                    BuildTopicBundles(namespaces, addressingLogic);
                }

                return new TopologySection
                {
                    Entities = SelectFirstTopicFromBundle(topics),
                    Namespaces = namespaces
                };
            });
        }

        IEnumerable<EntityInfo> SelectFirstTopicFromBundle(List<EntityInfo> entityInfos)
        {
            const int index = 0;
            yield return entityInfos[index];
        }

        public TopologySection DetermineSendDestination(string destination)
        {
            return sendDestinations.GetOrAdd(destination, d =>
            {
                var partitioningStrategy = (INamespacePartitioningStrategy)container.Resolve(typeof(INamespacePartitioningStrategy));
                var addressingLogic = (AddressingLogic)container.Resolve(typeof(AddressingLogic));
                var defaultAlias = settings.Get<string>(WellKnownConfigurationKeys.Topology.Addressing.DefaultNamespaceAlias);

                var inputQueueAddress = addressingLogic.Apply(d, EntityType.Queue);

                RuntimeNamespaceInfo[] namespaces = null;
                if (inputQueueAddress.HasSuffix && inputQueueAddress.Suffix != defaultAlias) // sending to specific namespace
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
                    throw new Exception($"Could not determine namespace for destination `{d}`.");
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
            });

        }

        public TopologySection DetermineResourcesToSubscribeTo(Type eventType)
        {
            if (!subscriptions.ContainsKey(eventType))
            {
                subscriptions[eventType] = BuildSubscriptionHierarchy(eventType);
            }

            return subscriptions[eventType];
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

        TopologySection BuildSubscriptionHierarchy(Type eventType)
        {
            var partitioningStrategy = (INamespacePartitioningStrategy)container.Resolve(typeof(INamespacePartitioningStrategy));
            var endpointName = settings.EndpointName();
            var namespaces = partitioningStrategy.GetNamespaces(PartitioningIntent.Creating).ToArray();
            var addressingLogic = (AddressingLogic)container.Resolve(typeof(AddressingLogic));

            var sanitizedInputQueuePath = addressingLogic.Apply(endpointName, EntityType.Queue).Name;
            var sanitizedSubscriptionPath = addressingLogic.Apply(endpointName, EntityType.Subscription).Name;
            // rule name needs to be 1) based on event full name 2) unique 3) deterministic
            var ruleName = addressingLogic.Apply(eventType.FullName, EntityType.Rule).Name;

            if (!topics.Any())
            {
                BuildTopicBundles(namespaces, addressingLogic);
            }
            var subs = new List<SubscriptionInfo>();
            foreach (var topic in topics)
            {
                subs.AddRange(namespaces.Select(ns =>
                {
                    var sub = new SubscriptionInfo
                    {
                        Namespace = ns,
                        Type = EntityType.Subscription,
                        Path = sanitizedSubscriptionPath,
                        Metadata = new ForwardingTopologySubscriptionMetadata
                        {
                            Description = $"Events {endpointName} is subscribed to",
                            SubscriptionNameBasedOnEventWithNamespace = ruleName,
                            NamespaceInfo = ns,
                            SubscribedEventFullName = eventType.FullName
                        },
                        BrokerSideFilter = new SqlSubscriptionFilter(eventType),
                        ShouldBeListenedTo = false
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
                            Path = sanitizedInputQueuePath,
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

        void BuildTopicBundles(RuntimeNamespaceInfo[] namespaces, AddressingLogic addressingLogic)
        {
            var numberOfEntitiesInBundle = settings.Get<int>(WellKnownConfigurationKeys.Topology.Bundling.NumberOfEntitiesInBundle);
            var bundlePrefix = settings.Get<string>(WellKnownConfigurationKeys.Topology.Bundling.BundlePrefix);

            foreach (var @namespace in namespaces)
            {
                var numberOfTopicsFound = namespaceBundleConfigurations.Value.GetNumberOfTopicInBundle(@namespace.Alias);
                var numberOfTopicsToCreate = Math.Max(numberOfEntitiesInBundle, numberOfTopicsFound);
                for (var i = 1; i <= numberOfTopicsToCreate; i++)
                {
                    var topicEntity = new EntityInfo
                    {
                        Path = addressingLogic.Apply(bundlePrefix + i, EntityType.Topic).Name,
                        Type = EntityType.Topic,
                        Namespace = @namespace
                    };
                    topics.Add(topicEntity);
                }
            }
        }
    }
}