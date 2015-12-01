namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using System.IO;

    public class IncomingMessageDetails
    {
        readonly string messageId;
        readonly Dictionary<string, string> headers;
        readonly Stream bodyStream;

        public IncomingMessageDetails(string messageId, Dictionary<string, string> headers, Stream bodyStream)
        {
            this.messageId = messageId;
            this.headers = headers;
            this.bodyStream = bodyStream;
        }

        public string MessageId
        {
            get { return messageId; }
        }

        public Dictionary<string, string> Headers
        {
            get { return headers; }
        }

        public Stream BodyStream
        {
            get { return bodyStream; }
        }
    }
}