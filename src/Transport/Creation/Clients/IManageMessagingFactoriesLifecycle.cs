namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;

    public interface IManageMessagingFactoriesLifecycle
    {
        MessagingFactory Get(Address address);
    }
}