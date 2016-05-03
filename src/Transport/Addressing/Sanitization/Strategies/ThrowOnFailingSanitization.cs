namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;

    public class ThrowOnFailingSanitization : ISanitizationStrategy
    {
        IValidationStrategy validationStrategy;

        public ThrowOnFailingSanitization(IValidationStrategy validationStrategy)
        {
            this.validationStrategy = validationStrategy;
        }

        public string Sanitize(string entityPathOrName, EntityType entityType)
        {
            var pathOrName = "path";

            if (entityType == EntityType.Rule || entityType == EntityType.Subscription)
            {
                pathOrName = "name";
            }

            if (!validationStrategy.IsValid(entityPathOrName, entityType))
            {
                throw new Exception($"Invalid {entityType} {pathOrName} `{entityPathOrName}` that cannot be used with Azure Service Bus. {entityType} {pathOrName} exceeds maximum allowed length or contains invalid characters. " + 
                                    "Check for invalid characters, shorten the name, or use `Sanitization().UseStrategy<ISanitizationStrategy>()` configuration extension.");
            }

            return entityPathOrName;
        }
    }
}