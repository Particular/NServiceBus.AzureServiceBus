namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Collections.Generic;

    public interface INamespacePartitioningStrategy
    {
        IEnumerable<NamespaceInfo> GetNamespaces(string endpointName, Purpose purpose);
    }
}
