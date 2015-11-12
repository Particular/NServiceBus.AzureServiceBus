namespace NServiceBus.AzureServiceBus
{
    using System.Linq;
    using System.Threading.Tasks;
    using NServiceBus.ObjectBuilder;
    
    class TopologyCreator : ICreateTopology
    {
        readonly IBuilder builder;
        readonly IManageNamespaceManagerLifeCycle namespaces;

        public TopologyCreator(IBuilder builder, IManageNamespaceManagerLifeCycle namespaces)
        {
            this.builder = builder;
            this.namespaces = namespaces;
        }

        public async Task Create(TopologySection topology)
        {
            var queues = topology.Entities.Where(e => e.Type == EntityType.Queue);
            var topics = topology.Entities.Where(e => e.Type == EntityType.Topic);
            var subscriptions = topology.Entities.Where(e => e.Type == EntityType.Subscription);

            var queueCreator = (ICreateAzureServiceBusQueues)builder.Build(typeof(ICreateAzureServiceBusQueues));
            foreach (var queue in queues)
            {
                await queueCreator.CreateAsync(queue.Path, namespaces.Get(queue.Namespace.ConnectionString)).ConfigureAwait(false);
            }

            var topicCreator = (ICreateAzureServiceBusTopics)builder.Build(typeof(ICreateAzureServiceBusTopics));
            foreach (var topic in topics)
            {
                await topicCreator.CreateAsync(topic.Path, namespaces.Get(topic.Namespace.ConnectionString)).ConfigureAwait(false);
            }

            var subscriptionCreator = (ICreateAzureServiceBusSubscriptions)builder.Build(typeof(ICreateAzureServiceBusSubscriptions));
            foreach (var subscription in subscriptions)
            {
                var topic = subscription.RelationShips.First(r => r.Type == EntityRelationShipType.Subscription);
                await subscriptionCreator.CreateAsync(topic.Target.Path, subscription.Path, namespaces.Get(subscription.Namespace.ConnectionString)).ConfigureAwait(false);
            }
        }
    }
}