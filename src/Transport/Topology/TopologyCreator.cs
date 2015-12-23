namespace NServiceBus.AzureServiceBus
{
    using System.Linq;
    using System.Threading.Tasks;
    
    class TopologyCreator : ICreateTopology
    {
        readonly ITransportPartsContainer container;
        readonly IManageNamespaceManagerLifeCycle namespaces;

        public TopologyCreator(ITransportPartsContainer container, IManageNamespaceManagerLifeCycle namespaces)
        {
            this.container = container;
            this.namespaces = namespaces;
        }

        public async Task Create(TopologySection topology)
        {
            var queues = topology.Entities.Where(e => e.Type == EntityType.Queue).ToList();
            var topics = topology.Entities.Where(e => e.Type == EntityType.Topic).ToList();
            var subscriptions = topology.Entities.Where(e => e.Type == EntityType.Subscription).ToList();

            if (queues.Any())
            {
                var queueCreator = (ICreateAzureServiceBusQueues)container.Resolve(typeof(ICreateAzureServiceBusQueues));
                foreach (var queue in queues)
                {
                    await queueCreator.Create(queue.Path, namespaces.Get(queue.Namespace.ConnectionString)).ConfigureAwait(false);
                }
            }

            if (topics.Any())
            {
                var topicCreator = (ICreateAzureServiceBusTopics)container.Resolve(typeof(ICreateAzureServiceBusTopics));
                foreach (var topic in topics)
                {
                    await topicCreator.Create(topic.Path, namespaces.Get(topic.Namespace.ConnectionString)).ConfigureAwait(false);
                }
            }

            if (subscriptions.Any())
            {
                var subscriptionCreator = (ICreateAzureServiceBusSubscriptions) container.Resolve(typeof(ICreateAzureServiceBusSubscriptions));
                foreach (var subscription in subscriptions)
                {
                    var topic = subscription.RelationShips.First(r => r.Type == EntityRelationShipType.Subscription);
                    var sqlFilter = (subscription as SubscriptionInfo)?.BrokerSideFilter.Serialize();
                    var metadata = (subscription as SubscriptionInfo)?.Metadata ?? new SubscriptionMetadata();
                    await subscriptionCreator.Create(topic.Target.Path, subscription.Path, metadata, sqlFilter, namespaces.Get(subscription.Namespace.ConnectionString)).ConfigureAwait(false);
                }
            }
        }
    }
}