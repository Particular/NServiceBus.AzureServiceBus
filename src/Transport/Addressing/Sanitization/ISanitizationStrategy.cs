namespace NServiceBus.AzureServiceBus.Addressing
{
    public interface ISanitizationStrategy
    {
        string Sanitize(string endpointName, EntityType entityType);
    }
}
