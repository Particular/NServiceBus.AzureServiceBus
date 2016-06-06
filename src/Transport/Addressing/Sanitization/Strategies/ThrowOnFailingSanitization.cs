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

            var validationResult = validationStrategy.IsValid(entityPathOrName, entityType);
            if (!validationResult.IsValid)
            {
                throw new Exception($"Invalid {entityType} {pathOrName} `{entityPathOrName}` that cannot be used with Azure Service Bus. Errors: " + string.Join("; ", validationResult.Errors) 
                                    + Environment.NewLine + "Use `Sanitization().UseStrategy<ISanitizationStrategy>()` configuration API to register a custom sanitization strategy if required.");
            }

            return entityPathOrName;
        }
    }
}