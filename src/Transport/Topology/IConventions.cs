namespace NServiceBus.Transport.AzureServiceBus
{
    using System;

    interface IConventions
    {
        bool IsMessageType(Type type);
    }
}