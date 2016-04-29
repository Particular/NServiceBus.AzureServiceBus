namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Text.RegularExpressions;
    using Topology.MetaModel;

    public class EndpointOrientedTopologySanitization : ISanitizationStrategy
    {
        IValidationStrategy validationStrategy;

        public EndpointOrientedTopologySanitization(IValidationStrategy validationStrategy)
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
            var rgx = new Regex(@"[^a-zA-Z0-9\-\._]");
            entityPathOrName = rgx.Replace(entityPathOrName, "");

            if (!validationStrategy.IsValid(entityPathOrName, entityType))
            {
                // turn long name into a guid
                entityPathOrName = MD5DeterministicNameBuilder.Build(entityPathOrName);
            }

            return entityPathOrName;
        }
    }

}