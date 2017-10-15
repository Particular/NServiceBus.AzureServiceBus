namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Runtime.Serialization;
    using Microsoft.ServiceBus.Messaging;

    /// <summary>
    /// <see cref="BrokeredMessage"/> is too large to be sent.
    /// </summary>
    [Serializable]
    public class MessageTooLargeException : Exception
    {
        /// <summary> </summary>
        public MessageTooLargeException()
        {
        }

        /// <summary> </summary>
        public MessageTooLargeException(string message) : base(message)
        {
        }

        /// <summary> </summary>
        public MessageTooLargeException(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary> </summary>
        protected MessageTooLargeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}