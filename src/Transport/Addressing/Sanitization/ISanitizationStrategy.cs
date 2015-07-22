namespace NServiceBus.AzureServiceBus.Addressing
{
    public interface ISanitizationStrategy
    {
        string Sanitize(string endpointname, EntityType type);
    }
}
