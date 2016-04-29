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

        public string Sanitize(string entityPath, EntityType entityType)
        {
            var pathOrName = "path";

            if (entityType == EntityType.Rule || entityType == EntityType.Subscription)
            {
                pathOrName = "name";
            }

            if (!validationStrategy.IsValid(entityPath, entityType))
            {
                throw new Exception($"Invalid {entityType} {pathOrName} `{entityPath}` that cannot be used with Azure Service Bus. {entityType} {pathOrName} exceeds length or contains invalid characters.");
            }

            return entityPath;
        }
    }
}