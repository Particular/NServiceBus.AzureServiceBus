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
            var address = new EntityAddress(value);
            var path = addresses.GetOrAdd(value, x =>
            {
                var newPath = composition.GetEntityPath(x, entityType);
                newPath = sanitizationStrategy.Sanitize(newPath, entityType);
                addresses.TryAdd(newPath, newPath);
                return newPath;
            });

            return new EntityAddress(path, address.Suffix);
        }

        readonly ICompositionStrategy composition;
        readonly ISanitizationStrategy sanitizationStrategy;
        ConcurrentDictionary<string, string> addresses = new ConcurrentDictionary<string, string>();
    }
}