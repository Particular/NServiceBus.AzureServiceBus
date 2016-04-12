namespace NServiceBus.AzureServiceBus
{
    using System;

    class ConventionsAdapter : IConventions
    {
        Conventions inner;

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