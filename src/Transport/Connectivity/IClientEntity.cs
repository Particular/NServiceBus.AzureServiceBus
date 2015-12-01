namespace NServiceBus.AzureServiceBus
{
    using Microsoft.ServiceBus;

    public interface IClientEntity
    {
        bool IsClosed { get; }
        
        RetryPolicy RetryPolicy { get; set; }
    }
}