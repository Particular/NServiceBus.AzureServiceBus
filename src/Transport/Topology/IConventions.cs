namespace NServiceBus.AzureServiceBus
{
    using System;

    interface IConventions
    {
        bool IsMessageType(Type type);
    }

    class ConventionsAdapter : IConventions
    {
        private readonly Conventions inner;

        public ConventionsAdapter(Conventions conventions)
        {
            inner = conventions;
        }

        public bool IsMessageType(Type type)
        {
            return inner.IsMessageType(type);
        }
    }
}