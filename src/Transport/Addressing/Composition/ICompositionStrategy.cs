namespace NServiceBus.Transport.AzureServiceBus
{
    /// <summary>
    /// Contract to implement a custom composition strategy to determine the path to an entity in the namespace.
    /// </summary>
    public interface ICompositionStrategy
    {
        /// <summary>
        /// Get entity path from its name
        /// </summary>
        string GetEntityPath(string entityName, EntityType entityType);
    }
}