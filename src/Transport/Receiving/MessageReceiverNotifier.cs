namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.Transports;

    class MessageReceiverNotifier : INotifyIncomingMessages
    {
        readonly IManageClientEntityLifeCycle clientEntities;

        public MessageReceiverNotifier(IManageClientEntityLifeCycle clientEntities)
        {
            this.clientEntities = clientEntities;
        }

        public void Initialize(Func<IncomingMessage, Task> callback)
        {
        }

        public void Start()
        {
        }

        public void Stop()
        {
        }
    }
}