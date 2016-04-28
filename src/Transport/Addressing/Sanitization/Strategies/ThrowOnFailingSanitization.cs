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
            if (!validationStrategy.IsValid(entityPath, entityType))
            {
                throw new Exception("The entity path {0} cannot be used as a path for azure servicebus entities");
            }

            return entityPath;
        }
    }
}