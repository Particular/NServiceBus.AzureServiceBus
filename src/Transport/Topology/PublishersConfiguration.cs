namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using NServiceBus.AzureServiceBus.TypesScanner;
    using NServiceBus.Settings;

    class PublishersConfiguration
    {
        private readonly IConventions _conventions;
        private readonly Dictionary<Type, List<string>> _publishers;

        public PublishersConfiguration(IConventions conventions, ReadOnlySettings settings)
        {
            _conventions = conventions;
            _publishers = new Dictionary<Type, List<string>>();

            if (settings.HasSetting(WellKnownConfigurationKeys.Topology.Publishers))
                settings
                    .Get<Dictionary<string, List<ITypesScanner>>>(WellKnownConfigurationKeys.Topology.Publishers)
                    .ToDictionary(x => x.Key, x => x.Value.SelectMany(scanner => scanner.Scan()))
                    .ToList()
                    .ForEach(x => Map(x.Key, x.Value));
        }

        public void Map(string publisherName, Type type)
        {
            var types = type
                .GetParentTypes()
                .Union(new[] { type })
                .Where(t => _conventions.IsMessageType(t))
                .ToArray();

            Array.ForEach(types, t => AddPublisherForType(publisherName, t));
        }

        public void Map(string publisherName, IEnumerable<Type> types)
        {
            foreach (var type in types)
                Map(publisherName, type);
        }

        public IEnumerable<string> GetPublishersFor(Type type)
        {
            if (!HasPublishersFor(type))
                throw new InvalidOperationException($"No publishers configured for `{type.FullName}`");

            return new ReadOnlyCollection<string>(_publishers[type]);
        }

        public bool HasPublishersFor(Type type)
        {
            return _publishers.ContainsKey(type);
        }

        private void AddPublisherForType(string publisherName, Type type)
        {
            List<string> publisherNames;
            if (!_publishers.TryGetValue(type, out publisherNames))
            {
                publisherNames = new List<string>();
                _publishers.Add(type, publisherNames);
            }

            if (!publisherNames.Contains(publisherName))
                publisherNames.Add(publisherName);
        }
    }
}