namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Text.RegularExpressions;

    public class EndpointOrientedTopologySanitization : ISanitizationStrategy
    {
        IValidationStrategy validationStrategy;

        public EndpointOrientedTopologySanitization(IValidationStrategy validationStrategy)
        {
            this.validationStrategy = validationStrategy;
        }

        public string Sanitize(string entityPathOrName, EntityType entityType)
        {
            // remove invalid characters
            var rgx = new Regex(@"[^a-zA-Z0-9\-\._]");
            entityPathOrName = rgx.Replace(entityPathOrName, "");

            var validationResult = validationStrategy.IsValid(entityPathOrName, entityType);
            if (!validationResult.IsValid)
            {
                // turn long name into a guid
                entityPathOrName = MD5DeterministicNameBuilder.Build(entityPathOrName);
            }

            return entityPathOrName;
        }
    }

}