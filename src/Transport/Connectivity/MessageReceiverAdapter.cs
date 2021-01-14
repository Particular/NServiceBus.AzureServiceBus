namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;

    class MessageReceiverAdapter : IMessageReceiverInternal
    {
        public MessageReceiverAdapter(MessageReceiver receiver)
        {
            this.receiver = receiver;
        }

        public bool IsClosed => receiver.IsClosed;

        public RetryPolicy RetryPolicy
        {
            get => receiver.RetryPolicy;
            set => receiver.RetryPolicy = value;
        }

        public int PrefetchCount
        {
            get => receiver.PrefetchCount;
            set => receiver.PrefetchCount = value;
        }

        public ReceiveMode Mode => receiver.Mode;

        public void OnMessage(Func<BrokeredMessage, Task> callback, OnMessageOptions options) => receiver.OnMessageAsync(callback, options);

        public Task CloseAsync() => receiver.CloseAsync();

        public Task CompleteBatchAsync(IEnumerable<Guid> lockTokens) => receiver.CompleteBatchAsync(lockTokens);

        MessageReceiver receiver;
    }
}