namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;

    class AzureServicebusQueueClientCreator : ICreateQueueClients
    {
        Configure config;

        public AzureServicebusQueueClientCreator(Configure config)
        {
            this.config = config;
        }

        public int BatchSize { get; set; }

        public QueueClient Create(QueueDescription description, MessagingFactory factory)
        {
            return Create(description.Path, factory);
        }

        public QueueClient Create(string description, MessagingFactory factory)
        {
            var client = factory.CreateQueueClient(description, ShouldRetry() ? ReceiveMode.PeekLock : ReceiveMode.ReceiveAndDelete);
            client.PrefetchCount = BatchSize;
            return client;
        }

        bool ShouldRetry()
        {
            return (bool)config.Settings.Get("Transactions.Enabled");
        }
    }
}