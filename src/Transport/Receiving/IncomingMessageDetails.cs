namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using System.IO;

    public class IncomingMessageDetails
    {
        public IncomingMessageDetails(string messageId, Dictionary<string, string> headers, Stream bodyStream)
        {
            MessageId = messageId;
            Headers = headers;
            BodyStream = bodyStream;
        }

        public string MessageId { get; }

        public Dictionary<string, string> Headers { get; }

        public Stream BodyStream { get; }
    }
}