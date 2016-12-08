namespace NServiceBus.Transport.AzureServiceBus
{
    using Microsoft.ServiceBus;

    interface IClientEntityInternal
    {
        bool IsClosed { get; }

        RetryPolicy RetryPolicy { get; set; }
    }
}