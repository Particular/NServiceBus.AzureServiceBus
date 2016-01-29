namespace NServiceBus.AzureServiceBus
{
    using System.Threading.Tasks;
    using NServiceBus.Transports;

    class TransportResourcesCreator : ICreateQueues
    {
        ITopologySectionManager sections;
        ICreateTopology topologyCreator;
        bool resourcesCreated = false;

        public TransportResourcesCreator(ICreateTopology topologyCreator, ITopologySectionManager sections)
        {
            this.topologyCreator = topologyCreator;
            this.sections = sections;
        }

        public async Task CreateQueueIfNecessary(QueueBindings queueBindings, string identity)
        {
            if (resourcesCreated) return;

            var receiveResources = sections.DetermineResourcesToCreate();
            await topologyCreator.Create(receiveResources).ConfigureAwait(false);

            resourcesCreated = true;
        }
    }
}