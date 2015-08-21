namespace NServiceBus.AzureServiceBus
{
    using System;
    using Microsoft.ServiceBus;

    public interface INamespaceManager
    {
        NamespaceManagerSettings Settings { get; }
        Uri Address { get; }
    }
}