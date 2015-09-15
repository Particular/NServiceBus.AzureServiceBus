namespace NServiceBus.AzureServiceBus.Addressing
{
    public class ThrowOnFailingSanitizationStrategy : ISanitizationStrategy
    {
        IValidationStrategy _validationStrategy;

        public ThrowOnFailingSanitizationStrategy(IValidationStrategy validationStrategy)
        {
            _validationStrategy = validationStrategy;
        }

        public string Sanitize(string endpointName, EntityType entityType)
        {
            if (!_validationStrategy.IsValid(endpointName, entityType))
            {
                throw new EndpointValidationException("The endpoint name {0} cannot be used as a name for azure servicebus entities");
            }

            return endpointName;
        }
    }
}