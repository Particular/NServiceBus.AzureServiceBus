namespace NServiceBus.Transport.AzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;

    class BrokeredMessageReceiveContextInternal : ReceiveContextInternal
    {
        public BrokeredMessageReceiveContextInternal(BrokeredMessage message, EntityInfoInternal entity, ReceiveMode receiveMode)
        {
            IncomingBrokeredMessage = message;
            Entity = entity;
            ReceiveMode = receiveMode;
        }

        public BrokeredMessage IncomingBrokeredMessage { get; }

        public EntityInfoInternal Entity { get; }

        // Dispatcher needs to compare this with requested consistency guarantees, cannot do default (postponed) dispatch if there is no completion step (ReceiveAndDelete)
        public ReceiveMode ReceiveMode { get; }

        // while recovering, send via must be avoided as it will be rolled back
        public bool Recovering { get; set; }

    }
}