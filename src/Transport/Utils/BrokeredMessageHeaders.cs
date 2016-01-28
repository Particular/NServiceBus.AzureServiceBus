namespace NServiceBus.AzureServiceBus
{
    internal static class BrokeredMessageHeaders
    {
        public const string TransportEncoding = "NServiceBus.Transport.Encoding";
        public const string EstimatedMessageSize = "NServiceBus.Transport.EstimatedSize";
    }
}