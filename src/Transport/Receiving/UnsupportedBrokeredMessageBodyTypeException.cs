namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Runtime.Serialization;
    using Microsoft.ServiceBus.Messaging;

    /// <summary>
    /// Exception thrown when an unsupported <see cref="BrokeredMessage"/> encoding is encountered.
    /// </summary>
    [Serializable]
    public class UnsupportedBrokeredMessageBodyTypeException : Exception
    {
        /// <summary></summary>
        public UnsupportedBrokeredMessageBodyTypeException()
        {
        }

        /// <summary></summary>
        public UnsupportedBrokeredMessageBodyTypeException(string message) : base(message)
        {
        }

        /// <summary></summary>
        public UnsupportedBrokeredMessageBodyTypeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        /// <summary></summary>
        protected UnsupportedBrokeredMessageBodyTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}