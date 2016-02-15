namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using NServiceBus.Settings;

    public class RoundRobinNamespacePartitioningStrategy : INamespacePartitioningStrategy
    {
        readonly CircularBuffer<Tuple<string, string>> _connectionstrings;

        public RoundRobinNamespacePartitioningStrategy(ReadOnlySettings settings)
        {
            Dictionary<string, string> connectionstrings;
            if (!settings.TryGet(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, out connectionstrings))
            {
                connectionstrings = new Dictionary<string, string>();
            }

            if (connectionstrings.Count <= 1)
            {
                throw new ConfigurationErrorsException("The 'RoundRobin' namespace partitioning strategy requires more than one namespace, please configure multiple azure servicebus namespaces");
            }

            _connectionstrings = new CircularBuffer<Tuple<string, string>>(connectionstrings.Count);
            foreach (var connectionstring in connectionstrings)
            {
                _connectionstrings.Put(new Tuple<string, string>(connectionstring.Key, connectionstring.Value));
            }
        }

        public IEnumerable<NamespaceInfo> GetNamespaces(string endpointName, PartitioningIntent partitioningIntent)
        {
            if (partitioningIntent == PartitioningIntent.Sending)
            {
                var tuple = _connectionstrings.Get();
                yield return new NamespaceInfo(tuple.Item1, tuple.Item2, NamespaceMode.Active);
            }

            if (partitioningIntent == PartitioningIntent.Receiving || partitioningIntent == PartitioningIntent.Creating)
            {
                var mode = NamespaceMode.Active;
                for(var i = 0; i < _connectionstrings.Size; i++)
                {
                    var tuple = _connectionstrings.Get();
                    yield return new NamespaceInfo(tuple.Item1, tuple.Item2, mode);
                    mode = NamespaceMode.Passive;
                }
            }
        }
    }
}