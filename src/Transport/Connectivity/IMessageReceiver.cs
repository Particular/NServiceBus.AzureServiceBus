namespace NServiceBus.Transport.AzureServiceBus
{ 
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IMessageReceiver : IClientEntity
    {
        int PrefetchCount { get; set; }
        ReceiveMode Mode { get; }
        void OnMessage(Func<BrokeredMessage, Task> callback, OnMessageOptions options);
        Task CloseAsync();
        Task CompleteBatchAsync(IEnumerable<Guid> lockTokens);
    }
}