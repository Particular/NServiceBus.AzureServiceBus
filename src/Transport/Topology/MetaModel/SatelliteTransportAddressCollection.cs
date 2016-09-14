namespace NServiceBus.AzureServiceBus.Topology.MetaModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using Logging;

    class SatelliteTransportAddressCollection : IEnumerable<string>
    {
        static ILog log = LogManager.GetLogger<SatelliteTransportAddressCollection>();
        List<string> list;

        public SatelliteTransportAddressCollection()
        {
            list = new List<string>();
        }

        public void Add(string satelliteTransportAddress)
        {
            if (InternalContains(satelliteTransportAddress))
            {
                return;
            }

            log.Debug($"Address '{satelliteTransportAddress}' registered as a satellite tranposrt address.");
            list.Add(satelliteTransportAddress);
        }

        /// <summary>
        /// Does transport address belong to a known satellite?
        /// </summary>
        /// <remarks>Performs <see cref="StringComparison.OrdinalIgnoreCase"/> equality check.</remarks>
        public bool Contains(string transportAddress)
        {
            return InternalContains(transportAddress);
        }

        bool InternalContains(string candidate)
        {
            return list.Any(x => x.Equals(candidate, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerator<string> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}