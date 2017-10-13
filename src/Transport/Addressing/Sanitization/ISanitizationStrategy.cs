namespace NServiceBus.Transport.AzureServiceBus
{
    /// <summary>
    /// Contract to implement custom sanitization strategy.
    /// </summary>
    public interface ISanitizationStrategy
    {
        /// <summary>
        /// Sanitizes entity path or name. Value depends on the <param name="entityType"/> parameter to determine what needs to be done with <param name="entityPathOrName" /> value.
        /// </summary>
        /// <returns>Value to be used as entity path name.</returns>
        string Sanitize(string entityPathOrName, EntityType entityType);
    }
}