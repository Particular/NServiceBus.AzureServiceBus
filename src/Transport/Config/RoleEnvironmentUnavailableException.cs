namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class RoleEnvironmentUnavailableException : Exception
    {
        public RoleEnvironmentUnavailableException() { }

        public RoleEnvironmentUnavailableException(string message) : base(message) { }

        public RoleEnvironmentUnavailableException(string message, Exception inner) : base(message, inner) { }

        protected RoleEnvironmentUnavailableException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}