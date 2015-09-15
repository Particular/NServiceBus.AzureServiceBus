namespace NServiceBus.AzureServiceBus.Addressing
{
    using System.Collections.Generic;
    using System.Configuration;
    using NServiceBus.Settings;

    public class RoundRobinNamespacePartitioningStrategy : INamespacePartitioningStrategy
    {
        readonly CircularBuffer<string> _connectionstrings;

        public RoundRobinNamespacePartitioningStrategy(ReadOnlySettings settings)
        {
            List<string> connectionstrings;
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, out connectionstrings))
            {
                connectionstrings = new List<string>();
            }

            if (connectionstrings.Count <= 1)
            {
                throw new ConfigurationErrorsException("The 'RoundRobin' namespace partitioning strategy requires more than one namespace, please configure multiple azure servicebus namespaces");
            }

            _connectionstrings = new CircularBuffer<string>(connectionstrings.Count);
            foreach (var connectionstring in connectionstrings)
            {
                _connectionstrings.Put(connectionstring);
            }
        }

        public IEnumerable<NamespaceInfo> GetNamespaces(string endpointName, Purpose purpose)
        {
            if (purpose == Purpose.Sending)
            {
                yield return new NamespaceInfo(_connectionstrings.Get(), NamespaceMode.Active);    
            }

            if (purpose == Purpose.Receiving || purpose == Purpose.Creating)
            {
                var mode = NamespaceMode.Active;
                for(var i = 0; i < _connectionstrings.Size; i++)
                {
                    yield return new NamespaceInfo(_connectionstrings.Get(), mode);
                    mode = NamespaceMode.Passive;
                }
            }
        }
    }
}