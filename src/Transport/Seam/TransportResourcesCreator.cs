namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;

    class TransportResourcesCreator : ICreateQueues
    {
        public TransportResourcesCreator(TopologyCreator topologyCreator, ITopologySectionManagerInternal sections)
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

            await topologyCreator.AssertManagedRights().ConfigureAwait(false);

            var receiveResources = sections.DetermineResourcesToCreate(queueBindings);
            await topologyCreator.Create(receiveResources).ConfigureAwait(false);

            resourcesCreated = true;
        }

        ITopologySectionManagerInternal sections;
        TopologyCreator topologyCreator;
        bool resourcesCreated;
    }
}