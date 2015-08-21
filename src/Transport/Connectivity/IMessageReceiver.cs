namespace NServiceBus.AzureServiceBus
{

    public interface IMessageReceiver : IClientEntity
    {
        int PrefetchCount { get; set; }
    }
}