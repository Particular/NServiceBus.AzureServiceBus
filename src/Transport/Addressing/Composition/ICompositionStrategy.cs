namespace NServiceBus.Transport.AzureServiceBus
{
    using Settings;

    public interface ICompositionStrategy
    {
        void Initialize(ReadOnlySettings settings);

        string GetEntityPath(string entityName, EntityType entityType);
    }
}