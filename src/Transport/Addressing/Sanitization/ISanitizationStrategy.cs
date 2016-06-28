namespace NServiceBus.AzureServiceBus.Addressing
{
    using Settings;

    public interface ISanitizationStrategy
    {
        void SetDefaultRules(SettingsHolder settings);

        string Sanitize(string entityPathOrName, EntityType entityType);
    }
}
