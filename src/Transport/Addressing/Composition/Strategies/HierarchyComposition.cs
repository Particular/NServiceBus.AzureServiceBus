namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;

    public class HierarchyComposition : ICompositionStrategy
    {
        Func<string, string> pathGenerator;

        public void SetPathGenerator(Func<string, string> pathGenerator)
        {
            this.pathGenerator = pathGenerator;
        }

        public string GetEntityPath(string entityname, EntityType entityType)
        {
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
    }
}