namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using NServiceBus.Logging;

    public class AzureServiceBusTopologyCreator: NServiceBus.Transports.ICreateQueues
    {
        ITopology topology;
        ILog logger = LogManager.GetLogger(typeof(AzureServiceBusTopologyCreator));

        public AzureServiceBusTopologyCreator(ITopology topology)
        {
            this.topology = topology;
        }

        public void CreateQueueIfNecessary(Address address, string account)
        {
            logger.InfoFormat("Going to create topology for address '{0}' if needed", address.Queue);

            topology.Create(address);
        }
    }
}