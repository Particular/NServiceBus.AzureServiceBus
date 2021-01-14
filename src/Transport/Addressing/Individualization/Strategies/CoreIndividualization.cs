namespace NServiceBus
{
    using Transport.AzureServiceBus;

    /// <summary>
    /// Strategy based on individualization logic defined in the NServiceBus core framework.
    /// </summary>
    public class CoreIndividualization : IIndividualizationStrategy
    {
        internal CoreIndividualization()
        {
        }

        /// <summary>
        /// Return <param name="endpointName"> as-is.</param>
        /// </summary>
        public string Individualize(string endpointName) => endpointName;
    }
}