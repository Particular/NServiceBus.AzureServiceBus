namespace NServiceBus.Transport.AzureServiceBus
{
    using Microsoft.ServiceBus;

    [ObsoleteEx(Message = "Internal contract that shouldn't be exposed.", TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IClientEntity
    {
        bool IsClosed { get; }

        RetryPolicy RetryPolicy { get; set; }
    }
}