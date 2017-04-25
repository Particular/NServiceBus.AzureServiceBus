namespace NServiceBus.AzureServiceBus.Topology.MetaModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    class NamespaceBundleConfigurations : IEnumerable<NamespaceBunleInfo>
    {
        List<NamespaceBunleInfo> namespaceBundles;

        public NamespaceBundleConfigurations()
        {
            namespaceBundles = new List<NamespaceBunleInfo>();
        }

        public void Add(string namespaceAlias, int numberOfTopicsInBundle)
        {
            var bunleInfo = namespaceBundles.SingleOrDefault(x => x.Alias.Equals(namespaceAlias, StringComparison.OrdinalIgnoreCase));
            if (bunleInfo != null && bunleInfo.NumberOfTopicsInBundle < numberOfTopicsInBundle)
            {
                throw new Exception($"Duplicate namespace alias `{namespaceAlias}` was added to the {nameof(NamespaceBundleConfigurations)}. Namespace aliases should be unique.");
            }

            namespaceBundles.Add(new NamespaceBunleInfo(namespaceAlias, numberOfTopicsInBundle));
        }

        public int GetNumberOfTopicInBundle(string namespaceAlias)
        {
            try
            {
                var selected = namespaceBundles.Single(x => x.Alias.Equals(namespaceAlias, StringComparison.OrdinalIgnoreCase));
                return selected.NumberOfTopicsInBundle;
            }
            catch (InvalidOperationException exception)
            {
                throw new KeyNotFoundException($"Namespace with alias `{namespaceAlias}` hasn't been registered", exception);
            }
        }

        public IEnumerator<NamespaceBunleInfo> GetEnumerator()
        {
            return namespaceBundles.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}