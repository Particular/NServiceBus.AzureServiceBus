namespace NServiceBus.Transport.AzureServiceBus
{
    class AddressingLogic
    {
        public AddressingLogic(ISanitizationStrategy sanitizationStrategy, ICompositionStrategy composition)
        {
            this.sanitizationStrategy = sanitizationStrategy;
            this.composition = composition;
        }

        public EntityAddress Apply(string value, EntityType entityType)
        {
            var originalAddress = new EntityAddress(value);

            var path = composition.GetEntityPath(originalAddress.Name, entityType);
            path = sanitizationStrategy.Sanitize(path, entityType);

            return new EntityAddress(path, originalAddress.Suffix);
        }

        readonly ICompositionStrategy composition;
        readonly ISanitizationStrategy sanitizationStrategy;
    }
}