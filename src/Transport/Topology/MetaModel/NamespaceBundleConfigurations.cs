namespace NServiceBus.AzureServiceBus.Topology.MetaModel
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    class NamespaceBundleConfigurations : IEnumerable<NamespaceBundleInfo>
    {
        public NamespaceBundleConfigurations()
        {
            namespaceBundles = new List<NamespaceBundleInfo>();
        }

        public IEnumerator<NamespaceBundleInfo> GetEnumerator()
        {
            return namespaceBundles.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(string namespaceAlias, int numberOfTopicsInBundle)
        {
            var bundleInfo = namespaceBundles.SingleOrDefault(x => x.Alias.Equals(namespaceAlias, StringComparison.OrdinalIgnoreCase));
            if (bundleInfo != null && bundleInfo.NumberOfTopicsInBundle < numberOfTopicsInBundle)
            {
                throw new Exception($"Duplicate namespace alias `{namespaceAlias}` was added to the {nameof(NamespaceBundleConfigurations)}. Namespace aliases should be unique.");
            }

            namespaceBundles.Add(new NamespaceBundleInfo(namespaceAlias, numberOfTopicsInBundle));
        }

        public int GetNumberOfTopicInBundle(string namespaceAlias)
        {
            var selected = namespaceBundles.SingleOrDefault(x => x.Alias.Equals(namespaceAlias, StringComparison.OrdinalIgnoreCase));
            return selected?.NumberOfTopicsInBundle ?? 1;
        }

        List<NamespaceBundleInfo> namespaceBundles;
    }
}