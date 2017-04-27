namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Linq;
    using System.Threading.Tasks;
    using NServiceBus.AzureServiceBus.Topology.MetaModel;

    class NumberOfTopicsInBundleCheck
    {
        readonly IManageNamespaceManagerLifeCycle manageNamespaceManagerLifeCycle;
        readonly NamespaceConfigurations namespaceConfigurations;
        readonly NamespaceBundleConfigurations namespaceBundleConfigurations;
        readonly string bundlePrefix;

        public NumberOfTopicsInBundleCheck(IManageNamespaceManagerLifeCycle manageNamespaceManagerLifeCycle, NamespaceConfigurations namespaceConfigurations, NamespaceBundleConfigurations namespaceBundleConfigurations, string bundlePrefix)
        {
            this.manageNamespaceManagerLifeCycle = manageNamespaceManagerLifeCycle;
            this.namespaceConfigurations = namespaceConfigurations;
            this.namespaceBundleConfigurations = namespaceBundleConfigurations;
            this.bundlePrefix = bundlePrefix;
        }

        public async Task Run()
        {
            foreach (var namespaceConfiguration in namespaceConfigurations)
            {
                var namespaceManager = manageNamespaceManagerLifeCycle.Get(namespaceConfiguration.Alias);
                var filter = $"startswith(path, '{bundlePrefix}') eq true";
                var namespaceManagerThatCanQueryAndFilterTopics = namespaceManager as NamespaceManagerAdapter;
                // if user has provided an implementation of INamespaceManager, skip the checks all together
                if (namespaceManagerThatCanQueryAndFilterTopics == null)
                {
                    return;
                }
                var foundTopics = await namespaceManagerThatCanQueryAndFilterTopics.GetTopics(filter).ConfigureAwait(false);
                namespaceBundleConfigurations.Add(namespaceConfiguration.Alias, foundTopics.Count());
            }
        }
    }
}