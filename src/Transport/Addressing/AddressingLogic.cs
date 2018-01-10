namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Concurrent;

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
            if (address.HasSuffix)
            {
                return address;
            }

            var path = cachedPaths.GetOrAdd(value, x =>
            {
                var newPath = composition.GetEntityPath(x, entityType);
                newPath = sanitizationStrategy.Sanitize(newPath, entityType);
                cachedPaths.TryAdd(newPath, newPath);
                return newPath;
            });

            return new EntityAddress(path, address.Suffix);
        }

        readonly ICompositionStrategy composition;
        readonly ISanitizationStrategy sanitizationStrategy;
        ConcurrentDictionary<string, string> cachedPaths = new ConcurrentDictionary<string, string>();
    }
}