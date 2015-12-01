namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class EndpointValidationException : Exception
    {
        public EndpointValidationException()
        {
        }

        public EndpointValidationException(string message) : base(message)
        {
        }

        public EndpointValidationException(string message, Exception inner) : base(message, inner)
        {
        }

        protected EndpointValidationException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}