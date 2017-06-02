namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Linq;
    using System.Threading.Tasks;
    using NServiceBus.AzureServiceBus.Topology.MetaModel;

    static class NumberOfTopicsInBundleCheck
    {
        public static async Task<NamespaceBundleConfigurations> Run(IManageNamespaceManagerLifeCycleInternal manageNamespaceManagerLifeCycle, NamespaceConfigurations namespaceConfigurations, string bundlePrefix)
        {
            var namespaceBundleConfigurations = new NamespaceBundleConfigurations();

            foreach (var namespaceConfiguration in namespaceConfigurations)
            {
                var namespaceManager = manageNamespaceManagerLifeCycle.Get(namespaceConfiguration.Alias);
                var namespaceManagerThatCanQueryAndFilterTopics = namespaceManager as NamespaceManagerAdapterInternal;

                // if user has provided an implementation of INamespaceManager, skip the checks all together
                if (namespaceManagerThatCanQueryAndFilterTopics == null)
                {
                    break;
                }

                var numberOfTopics = 1;
                if (await namespaceManagerThatCanQueryAndFilterTopics.CanManageEntities().ConfigureAwait(false))
                {
                    var filter = $"startswith(path, '{bundlePrefix}') eq true";
                    var foundTopics = await namespaceManagerThatCanQueryAndFilterTopics.GetTopics(filter).ConfigureAwait(false);
                    numberOfTopics = foundTopics.Count();
                }
                namespaceBundleConfigurations.Add(namespaceConfiguration.Alias, numberOfTopics);
            }

            return namespaceBundleConfigurations;
        }
    }
}