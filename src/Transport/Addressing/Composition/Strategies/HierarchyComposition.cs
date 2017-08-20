namespace NServiceBus
{
    using System;
    using Settings;
    using Transport.AzureServiceBus;

    public class HierarchyComposition : ICompositionStrategy
    {
        public void Initialize(ReadOnlySettings settings)
        {
            pathGenerator = settings.Get<Func<string, string>>(WellKnownConfigurationKeys.Topology.Addressing.Composition.HierarchyCompositionPathGenerator);
        }

        public string GetEntityPath(string entityName, EntityType entityType)
        {
            switch (entityType)
            {
                case EntityType.Queue:
                case EntityType.Topic:
                    return pathGenerator(entityName) + entityName;
                case EntityType.Subscription:
                case EntityType.Rule:
                    return entityName;
                default:
                    throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null);
            }
        }

        Func<string, string> pathGenerator;
    }
}