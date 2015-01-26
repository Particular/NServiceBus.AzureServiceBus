namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;

    /// <summary>
    /// 
    /// </summary>
    public interface ICreateTopicClients
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        TopicClient Create(TopicDescription topic, MessagingFactory factory);

        TopicClient Create(string topic, MessagingFactory factory);
    }
}