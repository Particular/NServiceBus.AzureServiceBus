namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class TransactionContainsTooManyMessages : Exception
    {
        public TransactionContainsTooManyMessages()
        {
        }

        public TransactionContainsTooManyMessages(string message = DefaultMessage) : base(message)
        {
        }

        public TransactionContainsTooManyMessages(string message, Exception inner) : base(message, inner)
        {
        }

        public TransactionContainsTooManyMessages(Exception inner) : base(DefaultMessage, inner)
        {
        }

        protected TransactionContainsTooManyMessages(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        const string DefaultMessage = "Azure Service Bus cannot send more than 100 messages in a signle transaction. Reduce number of messages sent out or reduce transport transaction mode.";
    }
}