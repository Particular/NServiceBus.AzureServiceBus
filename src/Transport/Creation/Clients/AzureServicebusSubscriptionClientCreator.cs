namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;

    class AzureServicebusSubscriptionClientCreator : ICreateSubscriptionClients
    {
        Configure config;
        public int BatchSize { get; set; }

        public AzureServicebusSubscriptionClientCreator(Configure config)
        {
            this.config = config;
        }

        public SubscriptionClient Create(SubscriptionDescription description, MessagingFactory factory)
        {
            var subscriptionClient = factory.CreateSubscriptionClient(description.TopicPath, description.Name, ShouldRetry() ? ReceiveMode.PeekLock : ReceiveMode.ReceiveAndDelete);
            subscriptionClient.PrefetchCount = BatchSize;
            return subscriptionClient;
        }

        bool ShouldRetry()
        {
            return (bool)config.Settings.Get("Transactions.Enabled");
        }
    }
}