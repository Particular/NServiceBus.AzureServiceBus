namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    class PublishersConfiguration
    {
        private readonly IConventions _conventions;
        private readonly Dictionary<Type, List<string>> _publishers;

        public PublishersConfiguration(IConventions conventions)
        {
            _conventions = conventions;
            _publishers = new Dictionary<Type, List<string>>();
        }

        public void Map(string publisherName, Type type)
        {
            var types = type
                .GetParentTypes()
                .Union(new [] { type })
                .Where(t => _conventions.IsMessageType(t))
                .ToArray();

            Array.ForEach(types, t => AddPublisherForType(publisherName, t));
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

        public IEnumerable<string> GetPublishersFor(Type type)
        {
            return _publishers.ContainsKey(type) 
                ? new ReadOnlyCollection<string>(_publishers[type]) 
                : new ReadOnlyCollection<string>(new string[0]);
        }

        public bool HasPublishersFor(Type type)
        {
            return _publishers.ContainsKey(type);
        }
    }

    internal static class TypeExtensions
    {
        public static IEnumerable<Type> GetParentTypes(this Type target)
        {
            foreach (var i in target.GetInterfaces())
                yield return i;

            var currentBaseType = target.BaseType;
            var objectType = typeof(object);
            while (currentBaseType != null && currentBaseType != objectType)
            {
                yield return currentBaseType;
                currentBaseType = currentBaseType.BaseType;
            }
        }
    }
}