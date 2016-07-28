namespace NServiceBus.AzureServiceBus
{
    static class BrokeredMessageHeaders
    {
        public const string TransportEncoding = "NServiceBus.Transport.Encoding";
        public const string EstimatedMessageSize = "NServiceBus.Transport.EstimatedSize";
        public const string Recovery = "NServiceBus.Transport.Recovery";
    }
}