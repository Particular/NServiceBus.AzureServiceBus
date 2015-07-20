namespace NServiceBus.AzureServiceBus.Addressing
{
    public interface IValidationStrategy
    {
        bool IsValid(string entitypath, EntityType entityType);
    }
}
