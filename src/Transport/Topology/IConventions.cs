namespace NServiceBus.AzureServiceBus
{
    using System;

    interface IConventions
    {
        bool IsMessageType(Type type);
    }

    class ConventionsAdapter : IConventions
    {
        private readonly Conventions _inner;

        public ConventionsAdapter(Conventions conventions)
        {
            _inner = conventions;
        }

        public bool IsMessageType(Type type)
        {
            return _inner.IsMessageType(type);
        }
    }
}