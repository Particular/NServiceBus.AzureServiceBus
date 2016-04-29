namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Text.RegularExpressions;
    using Topology.MetaModel;

    // TODO: remove when converted into a sample
    class AdjustmentSanitization : ISanitizationStrategy
    {
        IValidationStrategy validationStrategy;

        public AdjustmentSanitization(IValidationStrategy validationStrategy)
        {
            this.validationStrategy = validationStrategy;
        }

        public string Sanitize(string entityPathOrName, EntityType entityType)
        {
            if (entityType == EntityType.Queue)
            {
                var address = new EntityAddress(entityPathOrName);
                entityPathOrName = address.Name;
            }

            // remove invalid characters
            if (entityType == EntityType.Queue || entityType == EntityType.Topic)
            {
                var regexQueueAndTopicValidCharacters = new Regex(@"[^a-zA-Z0-9\-\._\/]");
                var regexLeadingAndTrailingForwardSlashes = new Regex(@"^\/|\/$");

                entityPathOrName = regexQueueAndTopicValidCharacters.Replace(entityPathOrName, "");
                entityPathOrName = regexLeadingAndTrailingForwardSlashes.Replace(entityPathOrName, "");
            }

            if (entityType == EntityType.Subscription || entityType == EntityType.Rule || entityType == EntityType.EventHub)
            {
                var rgx = new Regex(@"[^a-zA-Z0-9\-\._]");
                entityPathOrName = rgx.Replace(entityPathOrName, "");
            }

            if (!validationStrategy.IsValid(entityPathOrName, entityType))
            {
                entityPathOrName = SHA1DeterministicNameBuilder.Build(entityPathOrName);
            }

            return entityPathOrName;
        }
    }
}