namespace NServiceBus.Transport.AzureServiceBus
{
    /// <summary>
    /// Contract to implement a custom endpoint individualization.
    /// </summary>
    public interface IIndividualizationStrategy
    {
        /// <summary>
        /// Individualize endpoint name.
        /// </summary>
        /// <param name="endpointName">Original endpoint name.</param>
        /// <returns>Individualized endpoint name.</returns>
        string Individualize(string endpointName);
    }
}