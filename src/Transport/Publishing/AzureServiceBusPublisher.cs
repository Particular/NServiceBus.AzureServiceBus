namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using System.Transactions;
    using NServiceBus.Transports;
    using Unicast;
    using Unicast.Queuing;

    class AzureServiceBusPublisher : IPublishMessages
    {
        Configure config;
        ITopology topology;

        public AzureServiceBusPublisher(Configure config, ITopology topology)
        {
            this.config = config;
            this.topology = topology;
        }

        public void Publish(TransportMessage message, PublishOptions options)
        {
            var publisher = topology.GetPublisher(config.LocalAddress);

            if (publisher == null) throw new QueueNotFoundException { Queue = config.LocalAddress };

            if (!config.Settings.Get<bool>("Transactions.Enabled") || Transaction.Current == null)
                Publish(publisher, message, options);
            else
                Transaction.Current.EnlistVolatile(new SendResourceManager(() => Publish(publisher, message, options)), EnlistmentOptions.None);
        }

        void Publish(IPublishBrokeredMessages publisher, TransportMessage message, PublishOptions options)
        {
            using (var brokeredMessage = message.ToBrokeredMessage(options, config.Settings, config))
            {
                publisher.Publish(brokeredMessage);
            }
        }
    }
}