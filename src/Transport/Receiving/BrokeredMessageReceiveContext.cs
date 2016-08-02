namespace NServiceBus.AzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;

    public class BrokeredMessageReceiveContext : ReceiveContext
    {
        public BrokeredMessageReceiveContext(BrokeredMessage message, EntityInfo entity, ReceiveMode receiveMode)
        {
            IncomingBrokeredMessage = message;
            Entity = entity;
            ReceiveMode = receiveMode;
            CompletionCanBeBatched = true;
        }

        public BrokeredMessage IncomingBrokeredMessage { get; }

        public EntityInfo Entity { get; }

        // Dispatcher needs to compare this with requested consistency guarantees, cannot do default (postponed) dispatch if there is no completion step (ReceiveAndDelete)
        public ReceiveMode ReceiveMode { get; }

        // while recovering, send via must be avoided as it will be rolled back
        public bool Recovering { get; set; }

        public bool CompletionCanBeBatched { get; set; }
    }
}