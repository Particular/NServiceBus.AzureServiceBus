namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.Transports
{
    using Microsoft.ServiceBus.Messaging;

    /// <summary>
    /// 
    /// </summary>
    public interface ICreateQueues
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        QueueDescription Create(Address address);
    }
}