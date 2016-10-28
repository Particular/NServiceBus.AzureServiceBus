namespace NServiceBus.Transport.AzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;

    [ObsoleteEx(Message = "Internal contract that shouldn't be exposed.", TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
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

        // while recovering, send via must be avoided as it will be rolled back
        public bool Recovering { get; set; }

    }
}