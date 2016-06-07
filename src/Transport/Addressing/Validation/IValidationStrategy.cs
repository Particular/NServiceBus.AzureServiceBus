namespace NServiceBus.AzureServiceBus.Addressing
{
    public interface IValidationStrategy
    {
        ValidationResult IsValid(string entityPath, EntityType entityType);
    }
}
