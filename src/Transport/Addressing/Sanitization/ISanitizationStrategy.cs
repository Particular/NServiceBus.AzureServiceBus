namespace NServiceBus.AzureServiceBus.Addressing
{
    public interface ISanitizationStrategy
    {
        string Sanitize(string entityPath, EntityType entityType);
    }
}
