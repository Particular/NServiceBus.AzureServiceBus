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
            if (entityType == EntityType.Subscription)
            {
                return entityname;
            }

            return pathGenerator(entityname) + entityname;
        }
    }
}