namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    public interface IMessageReceiver : IClientEntity
    {
        int PrefetchCount { get; set; }
        ReceiveMode Mode { get; }
        void OnMessage(Func<BrokeredMessage, Task> callback, OnMessageOptions options);
        Task CloseAsync();
    }
}