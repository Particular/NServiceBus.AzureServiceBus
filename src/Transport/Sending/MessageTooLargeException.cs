namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class MessageTooLargeException : Exception
    {
        public MessageTooLargeException(){}

        public MessageTooLargeException(string message) : base(message){}

        public MessageTooLargeException(string message, Exception inner) : base(message, inner){}

        protected MessageTooLargeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}