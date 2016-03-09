namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Text.RegularExpressions;

    public class AdjustmentSanitization : ISanitizationStrategy
    {
        IValidationStrategy validationStrategy;

        public AdjustmentSanitization(IValidationStrategy validationStrategy)
        {
            this.validationStrategy = validationStrategy;
        }

        public string Sanitize(string entityPath, EntityType entityType)
        {
            // remove invalid characters

            if (entityType == EntityType.Queue || entityType == EntityType.Topic)
            {
                var regexQueueAndTopicValidCharacters = new Regex(@"[^a-zA-Z0-9\-\._\/]");
                var regexLeadingAndTrailingForwardSlashes = new Regex(@"^\/|\/$");

                entityPath = regexQueueAndTopicValidCharacters.Replace(entityPath, "");
                entityPath = regexLeadingAndTrailingForwardSlashes.Replace(entityPath, "");
            }

            if (entityType == EntityType.Subscription || entityType == EntityType.EventHub)
            {
                var rgx = new Regex(@"[^a-zA-Z0-9\-\._]");
                entityPath = rgx.Replace(entityPath, "");
            }

            if (!validationStrategy.IsValid(entityPath, entityType))
            {
                // turn long name into a guid
                entityPath = SHA1DeterministicNameBuilder.Build(entityPath);
            }

            return entityPath;
        }
    }
}