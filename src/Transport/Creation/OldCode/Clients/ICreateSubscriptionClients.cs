namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;

    /// <summary>
    /// 
    /// </summary>
    public interface ICreateSubscriptionClients
    {
        SubscriptionClient Create(SubscriptionDescription description, MessagingFactory factory);
    }
}