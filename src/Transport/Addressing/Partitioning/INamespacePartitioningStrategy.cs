namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Collections.Generic;

    public interface INamespacePartitioningStrategy
    {
        IEnumerable<string> GetConnectionStrings(string endpointName);
    }
}
