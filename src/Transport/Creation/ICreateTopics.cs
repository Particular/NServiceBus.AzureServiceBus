namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.Transports
{
    using Microsoft.ServiceBus.Messaging;

    /// <summary>
    /// 
    /// </summary>
    public interface ICreateTopics
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        TopicDescription Create(Address address);
    }
}