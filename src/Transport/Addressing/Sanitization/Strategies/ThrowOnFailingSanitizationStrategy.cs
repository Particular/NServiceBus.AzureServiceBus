namespace NServiceBus.AzureServiceBus.Addressing
{
    public class ThrowOnFailingSanitizationStrategy : ISanitizationStrategy
    {
        IValidationStrategy _validationStrategy;

        public ThrowOnFailingSanitizationStrategy(IValidationStrategy validationStrategy)
        {
            _validationStrategy = validationStrategy;
        }

        public string Sanitize(string entityPath, EntityType entityType)
        {
            if (!_validationStrategy.IsValid(entityPath, entityType))
            {
                throw new EndpointValidationException("The entity path {0} cannot be used as a path for azure servicebus entities");
            }

            return entityPath;
        }
    }
}