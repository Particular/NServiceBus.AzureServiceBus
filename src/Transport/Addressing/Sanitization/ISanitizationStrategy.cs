namespace NServiceBus.AzureServiceBus.Addressing
{
    public interface ISanitizationStrategy
    {
        string Sanitize(string entityPathOrName, EntityType entityType);
    }
}
