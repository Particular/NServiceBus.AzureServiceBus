namespace NServiceBus.AzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;

    public interface IMessageReceiver : IClientEntity
    {
        int PrefetchCount { get; set; }
        ReceiveMode Mode { get; }
    }
}