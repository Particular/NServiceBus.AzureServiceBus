namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.QueueAndTopicByEndpoint
{
    [ObsoleteEx(
       Message = "Use `Sanitization().UseStrategy<T>();` to provide sanitization strategy.",
       RemoveInVersion = "8.0",
       TreatAsErrorFromVersion = "7.0")]
    static class NamingConventions
    {
    }
}

namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    [ObsoleteEx(
        Message = "Use `BrokeredMessageBodyType();` to configure brokered bessage body serialization.",
        RemoveInVersion = "8.0",
        TreatAsErrorFromVersion = "7.0")]

    static class BrokeredMessageConverter
    {
    }
}

namespace NServiceBus.Config
{
    using System.Configuration;

    [ObsoleteEx(
        Message = "Use code-based configuration API.",
        RemoveInVersion = "8.0",
        TreatAsErrorFromVersion = "7.0")]
    public class AzureServiceBusQueueConfig : ConfigurationSection
    {
    }
}