namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;

    class AzureServicebusSubscriptionClientCreator : ICreateSubscriptionClients
    {
        Configure config;

        public AzureServicebusSubscriptionClientCreator(Configure config)
        {
            this.config = config;
        }

        public SubscriptionClient Create(SubscriptionDescription description, MessagingFactory factory)
        {
            return factory.CreateSubscriptionClient(description.TopicPath, description.Name, ShouldRetry() ? ReceiveMode.PeekLock : ReceiveMode.ReceiveAndDelete);
        }

        bool ShouldRetry()
        {
            return (bool)config.Settings.Get("Transactions.Enabled");
        }
    }
}