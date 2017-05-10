namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;
    using Transport;

    class TransportResourcesCreator : ICreateQueues
    {
        ITopologySectionManager sections;
        ICreateTopology topologyCreator;
        bool resourcesCreated;

        public TransportResourcesCreator(ICreateTopology topologyCreator, ITopologySectionManager sections)
        {
            this.topologyCreator = topologyCreator;
            this.sections = sections;
        }

        public async Task CreateQueueIfNecessary(QueueBindings queueBindings, string identity)
        {
            if (resourcesCreated)
            {
                return;
            }

            var receiveResources = sections.DetermineResourcesToCreate(queueBindings);
            await topologyCreator.Create(receiveResources).ConfigureAwait(false);

            resourcesCreated = true;
        }
    }
}