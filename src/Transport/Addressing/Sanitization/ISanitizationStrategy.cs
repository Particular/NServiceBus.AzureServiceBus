namespace NServiceBus.Transport.AzureServiceBus
{
    public interface ISanitizationStrategy
    {
        string Sanitize(string entityPathOrName, EntityType entityType);
    }
}
