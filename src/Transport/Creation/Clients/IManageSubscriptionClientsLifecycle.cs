namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;

    public interface IManageSubscriptionClientsLifecycle
    {
        TopicClient Get(Address address);
    }
}