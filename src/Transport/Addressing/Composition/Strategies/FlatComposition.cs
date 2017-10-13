namespace NServiceBus
{
    using Transport.AzureServiceBus;

    /// <summary>
    /// Composition strategy resulting in a flat namespace hierarchy.
    /// </summary>
    public class FlatComposition : ICompositionStrategy
    {
        /// <summary></summary>
        /// <returns>Returns <param name="entityName"/> as-is, irregardless of the <param name="entityType" />.</returns>
        public string GetEntityPath(string entityName, EntityType entityType)
        {
            return entityName;
        }
    }
}