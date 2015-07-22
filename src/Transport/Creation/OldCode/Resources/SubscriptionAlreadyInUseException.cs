namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class SubscriptionAlreadyInUseException : Exception
    {
        public SubscriptionAlreadyInUseException() { }

        public SubscriptionAlreadyInUseException(string message) : base(message) { }

        public SubscriptionAlreadyInUseException(string message, Exception inner) : base(message, inner) { }

        protected SubscriptionAlreadyInUseException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}