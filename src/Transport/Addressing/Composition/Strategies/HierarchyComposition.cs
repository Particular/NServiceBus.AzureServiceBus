namespace NServiceBus
{
    using System;
    using Settings;
    using Transport.AzureServiceBus;

    /// <summary>
    ///  Strategy to specify a path generator used to calculate the path for entities.
    /// <remarks>The path generator is invoked each time the transport wants to determine the location of a given entity.</remarks>
    /// </summary>
    public class HierarchyComposition : ICompositionStrategy
    {
        internal HierarchyComposition(ReadOnlySettings settings)
        {
            pathGenerator = settings.Get<Func<string, string>>(WellKnownConfigurationKeys.Topology.Addressing.Composition.HierarchyCompositionPathGenerator);
        }

        /// <summary>
        /// Calculate <param name="entityName" /> entity path using <param name="entityType" /> information and path generator specified at configuration time.
        /// </summary>
        /// <returns></returns>
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