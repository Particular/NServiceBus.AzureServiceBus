namespace NServiceBus.AzureServiceBus
{
    using System;

    interface IConventions
    {
        bool IsMessageType(Type type);
    }
}