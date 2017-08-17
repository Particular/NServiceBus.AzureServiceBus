namespace NServiceBus
{
    using System;
    using Settings;
    using Transport.AzureServiceBus;

    public class HierarchyComposition : ICompositionStrategy
    {
        internal HierarchyComposition(ReadOnlySettings settings)
        {
            this.settings = settings;
        }

        public string GetEntityPath(string entityname, EntityType entityType)
        {
            var pathGenerator = settings.Get<Func<string, string>>(WellKnownConfigurationKeys.Topology.Addressing.Composition.HierarchyCompositionPathGenerator);

            switch (entityType)
            {
                case EntityType.Queue:
                case EntityType.Topic:
                    return pathGenerator(entityname) + entityname;

                case EntityType.Subscription:
                case EntityType.Rule:
                    return entityname;

                default:
                    throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null);
            }
        }

        ReadOnlySettings settings;
    }
}