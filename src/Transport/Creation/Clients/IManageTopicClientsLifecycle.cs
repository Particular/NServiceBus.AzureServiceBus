namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;

    public interface IManageTopicClientsLifecycle
    {
        TopicClient Get(Address address);
    }
}