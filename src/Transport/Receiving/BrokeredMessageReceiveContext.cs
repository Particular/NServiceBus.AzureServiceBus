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
        }

        public BrokeredMessage IncomingBrokeredMessage { get; }

        public EntityInfo Entity { get; }

        // Dispatcher needs to compare this with requested consistency guarantees, cannot do default (postponed) dispatch if there is no completion step (ReceiveAndDelete)
        public ReceiveMode ReceiveMode { get; }
    }
}