namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Linq;
    using Addressing;
    using NServiceBus.Transports;
    using Settings;

    public class BasicTopologySectionManager : ITopologySectionManager
    {
        SettingsHolder settings;
        ITransportPartsContainer container;

        public BasicTopologySectionManager(SettingsHolder settings, ITransportPartsContainer container)
        {
            this.settings = settings;
            this.container = container;
        }

        public TopologySection DetermineReceiveResources(string inputQueue)
        {
            var partitioningStrategy = (INamespacePartitioningStrategy)container.Resolve(typeof(INamespacePartitioningStrategy));
            var sanitizationStrategy = (ISanitizationStrategy)container.Resolve(typeof(ISanitizationStrategy));

            var namespaces = partitioningStrategy.GetNamespaces(inputQueue, PartitioningIntent.Creating).ToArray();

            var inputQueuePath = sanitizationStrategy.Sanitize(inputQueue, EntityType.Queue);
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
            var sanitizationStrategy = (ISanitizationStrategy)container.Resolve(typeof(ISanitizationStrategy));

            var namespaces = partitioningStrategy.GetNamespaces(endpointName.ToString(), PartitioningIntent.Creating).ToArray();

            var inputQueuePath = sanitizationStrategy.Sanitize(endpointName.ToString(), EntityType.Queue);
            var inputQueues = namespaces.Select(n => new EntityInfo { Path = inputQueuePath, Type = EntityType.Queue, Namespace = n }).ToList();
            
            if(settings.HasExplicitValue<QueueBindings>())
            {
                var queueBindings = settings.Get<QueueBindings>();
                foreach (var n in namespaces)
                {
                    inputQueues.AddRange(queueBindings.ReceivingAddresses.Select(p => new EntityInfo
                    {
                        Path = p,
                        Type = EntityType.Queue,
                        Namespace = n
                    }));

                    inputQueues.AddRange(queueBindings.SendingAddresses.Select(p => new EntityInfo
                    {
                        Path = p,
                        Type = EntityType.Queue,
                        Namespace = n
                    }));
                }
            }

            var entities = inputQueues.ToArray();

            return new TopologySection
            {
                Namespaces = namespaces,
                Entities = entities
            };
        }

        public TopologySection DeterminePublishDestination(Type eventType)
        {
            throw new NotSupportedException("The current topologySectionManager does not support publishing via azure servicebus directly");
        }

        public TopologySection DetermineSendDestination(string destination)
        {
            var partitioningStrategy = (INamespacePartitioningStrategy)container.Resolve(typeof(INamespacePartitioningStrategy));
            var sanitizationStrategy = (ISanitizationStrategy)container.Resolve(typeof(ISanitizationStrategy));

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
            throw new NotSupportedException("The current topologySectionManager does not support azure servicebus subscriptions");
        }

        public TopologySection DetermineResourcesToUnsubscribeFrom(Type eventtype)
        {
            throw new NotSupportedException("The current topologySectionManager does not support azure servicebus subscriptions");
        }
    }
}