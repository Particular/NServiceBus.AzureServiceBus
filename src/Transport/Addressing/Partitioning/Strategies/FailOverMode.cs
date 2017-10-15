namespace NServiceBus.Transport.AzureServiceBus
{
    /// <summary>
    /// Namespace in use for <see cref="FailOverNamespacePartitioning"/>.
    /// </summary>
    public enum FailOverMode
    {
        /// <summary>Primary namespace is used.</summary>
        Primary,
        /// <summary>Secondary namespace is used.</summary>
        Secondary
    }
}