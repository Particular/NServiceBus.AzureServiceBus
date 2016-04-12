namespace NServiceBus.AzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;

    public class BrokeredMessageReceiveContext : ReceiveContext
    {
        public BrokeredMessage IncomingBrokeredMessage { get; set; }

        public EntityInfo Entity { get; set; }

        // Dispatcher needs to compare this with requested consistency guarantees, cannot do default (postponed) dispatch if there is no completion step (ReceiveAndDelete)
        public ReceiveMode ReceiveMode { get; set; }

    }
}