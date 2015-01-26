namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;

    class AzureServicebusTopicClientCreator : ICreateTopicClients
    {
        public TopicClient Create(TopicDescription topic, MessagingFactory factory)
        {
            return Create(topic.Path, factory);
        }

        public TopicClient Create(string topic, MessagingFactory factory)
        {
            return factory.CreateTopicClient(topic);
        }
    }
}