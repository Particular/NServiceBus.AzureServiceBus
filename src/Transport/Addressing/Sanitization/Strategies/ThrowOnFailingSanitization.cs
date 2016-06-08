namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;

    public class ThrowOnFailingSanitization : ISanitizationStrategy
    {
        public string Sanitize(string entityPathOrName, EntityType entityType, ValidationResult validationResult)
        {
            var pathOrName = "path";

            if (entityType == EntityType.Rule || entityType == EntityType.Subscription)
            {
                pathOrName = "name";
            }

            if (!validationResult.IsValid)
            {
                throw new Exception($"Invalid {entityType} entity {pathOrName} `{entityPathOrName}` that cannot be used with Azure Service Bus. Errors: " + string.Join("; ", validationResult.Errors) 
                                    + Environment.NewLine + "Use `Sanitization().UseStrategy<ISanitizationStrategy>()` configuration API to register a custom sanitization strategy if required.");
            }

            return entityPathOrName;
        }
    }
}