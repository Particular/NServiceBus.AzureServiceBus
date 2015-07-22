namespace NServiceBus.AzureServiceBus.Addressing
{
    public class ThrowOnInvalidSanitizationStrategy : ISanitizationStrategy
    {
        IValidationStrategy _validationStrategy;

        public ThrowOnInvalidSanitizationStrategy(IValidationStrategy validationStrategy)
        {
            _validationStrategy = validationStrategy;
        }

        public string Sanitize(string endpointname, EntityType type)
        {
            if (!_validationStrategy.IsValid(endpointname, type))
            {
                throw new EndpointValidationException("The endpoint name {0} cannot be used as a name for azure servicebus entities");
            }

            return endpointname;
        }
    }
}