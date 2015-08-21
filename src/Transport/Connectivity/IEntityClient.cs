namespace NServiceBus.AzureServiceBus
{
    public interface IEntityClient
    {
        bool IsClosed { get; }
    }
}