namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class UnsupportedBrokeredMessageBodyTypeException : Exception
    {
        public UnsupportedBrokeredMessageBodyTypeException()
        {
        }

        public UnsupportedBrokeredMessageBodyTypeException(string message) : base(message)
        {
        }

        public UnsupportedBrokeredMessageBodyTypeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnsupportedBrokeredMessageBodyTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}