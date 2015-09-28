namespace NServiceBus.AzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;

    /// <summary>
    /// Holds on to the the receive metadata
    /// - the original message so that it can be completed or abandoned when processing is done
    /// - the queue where it came from, so that sends can go via that queue to emulate send transactions 
    /// </summary>
    public abstract class ReceiveContext
    {
        

    }

    public class BrokeredMessageReceiveContext : ReceiveContext
    {
        public BrokeredMessage BrokeredMessage { get; set; }

        //TODO: replace by EntityInfo object
        public string EntityPath { get; set; }
        public string ConnectionString { get; set; }
    }
}