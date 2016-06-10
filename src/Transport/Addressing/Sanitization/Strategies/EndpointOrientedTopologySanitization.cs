namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Text.RegularExpressions;

    public class EndpointOrientedTopologySanitization : ISanitizationStrategy
    {
        public string Sanitize(string entityPathOrName, EntityType entityType, ValidationResult validationResult)
        {
            // remove invalid characters
            var rgx = new Regex(@"[^a-zA-Z0-9\-\._]");
            entityPathOrName = rgx.Replace(entityPathOrName, "");

            if (!validationResult.LengthIsValid)
            {
                // turn long name into a guid
                entityPathOrName = MD5DeterministicNameBuilder.Build(entityPathOrName);
            }

            return entityPathOrName;
        }
    }

}