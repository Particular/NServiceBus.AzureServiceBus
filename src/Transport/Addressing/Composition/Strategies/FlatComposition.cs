namespace NServiceBus
{
    using Settings;
    using Transport.AzureServiceBus;

    public class FlatComposition : ICompositionStrategy
    {
        public void Initialize(ReadOnlySettings settings)
        {
        }

        public string GetEntityPath(string entityName, EntityType entityType)
        {
            return entityName;
        }
    }
}