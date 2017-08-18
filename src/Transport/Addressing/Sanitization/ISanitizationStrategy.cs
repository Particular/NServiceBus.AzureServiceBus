namespace NServiceBus.Transport.AzureServiceBus
{
    using Settings;

    public interface ISanitizationStrategy
    {
        void Initialize(ReadOnlySettings settings);

        string Sanitize(string entityPathOrName, EntityType entityType);
    }
}