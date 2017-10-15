namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>Exception thrown by the transport when a batch contains more than 100 messages.</summary>
    [Serializable]
    public class TransactionContainsTooManyMessages : Exception
    {
        /// <summary></summary>
        public TransactionContainsTooManyMessages()
        {
        }

        /// <summary></summary>
        public TransactionContainsTooManyMessages(string message = DefaultMessage) : base(message)
        {
        }

        /// <summary></summary>
        public TransactionContainsTooManyMessages(string message, Exception inner) : base(message, inner)
        {
        }

        /// <summary></summary>
        public TransactionContainsTooManyMessages(Exception inner) : base(DefaultMessage, inner)
        {
        }

        /// <summary></summary>
        protected TransactionContainsTooManyMessages(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        const string DefaultMessage = "Azure Service Bus cannot send more than 100 messages in a signle transaction. Reduce number of messages sent out or reduce transport transaction mode.";
    }
}