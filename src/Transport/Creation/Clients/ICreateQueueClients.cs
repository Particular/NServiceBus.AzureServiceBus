namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;

    /// <summary>
    /// 
    /// </summary>
    public interface ICreateQueueClients
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        QueueClient Create(QueueDescription description, MessagingFactory factory);

        QueueClient Create(string description, MessagingFactory factory);
    }
}
