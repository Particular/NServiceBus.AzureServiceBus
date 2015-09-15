namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Collections.Generic;

    public interface INamespacePartitioningStrategy
    {
        IEnumerable<NamespaceInfo> GetNamespaceInfo(string endpointName);
    }
}
