namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
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

            string pathOrName;
            switch (entityType)
            {
                case EntityType.Queue:
                    pathOrName = cachedQueuePaths.GetOrAdd(value, x =>
                    {
                        var newPath = composition.GetEntityPath(x, entityType);
                        newPath = sanitizationStrategy.Sanitize(newPath, entityType);
                        cachedQueuePaths.TryAdd(newPath, newPath);
                        return newPath;
                    });
                    break;
                case EntityType.Topic:
                    pathOrName = cachedTopicPaths.GetOrAdd(value, x =>
                    {
                        var newPath = composition.GetEntityPath(x, entityType);
                        newPath = sanitizationStrategy.Sanitize(newPath, entityType);
                        cachedTopicPaths.TryAdd(newPath, newPath);
                        return newPath;
                    });
                    break;
                case EntityType.Subscription:
                    pathOrName = cachedSubscriptionName.GetOrAdd(value, x =>
                    {
                        var newPath = composition.GetEntityPath(x, entityType);
                        newPath = sanitizationStrategy.Sanitize(newPath, entityType);
                        cachedSubscriptionName.TryAdd(newPath, newPath);
                        return newPath;
                    });
                    break;
                case EntityType.Rule:
                    pathOrName = cachedRuleName.GetOrAdd(value, x =>
                    {
                        var newPath = composition.GetEntityPath(x, entityType);
                        newPath = sanitizationStrategy.Sanitize(newPath, entityType);
                        cachedRuleName.TryAdd(newPath, newPath);
                        return newPath;
                    });
                    break;
                default:
                    throw new Exception($"Unexpected entity type '{entityType}'.");
            }

            return new EntityAddress(pathOrName, address.Suffix);
        }

        readonly ICompositionStrategy composition;
        readonly ISanitizationStrategy sanitizationStrategy;
        ConcurrentDictionary<string, string> cachedQueuePaths = new ConcurrentDictionary<string, string>();
        ConcurrentDictionary<string, string> cachedTopicPaths = new ConcurrentDictionary<string, string>();
        ConcurrentDictionary<string, string> cachedSubscriptionName = new ConcurrentDictionary<string, string>();
        ConcurrentDictionary<string, string> cachedRuleName = new ConcurrentDictionary<string, string>();
    }
}