namespace NServiceBus.AzureServiceBus.Addressing
{
    public interface INamespacePartitioningStrategy
    {
        string GetConnectionString();
    }
}
