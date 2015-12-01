namespace NServiceBus.AzureServiceBus.Addressing
{
    public interface IValidationStrategy
    {
        bool IsValid(string entityPath, EntityType entityType);
    }
}
