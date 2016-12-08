namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;

    class IncomingMessageDetailsInternal
    {
        public IncomingMessageDetailsInternal(string messageId, Dictionary<string, string> headers, byte[] body)
        {
            MessageId = messageId;
            Headers = headers;
            Body = body;
        }

        public string MessageId { get; }

        public Dictionary<string, string> Headers { get; }

        public byte[] Body { get; }
    }
}