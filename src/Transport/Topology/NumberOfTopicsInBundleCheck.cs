namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.AzureServiceBus.Topology.MetaModel;

    static class NumberOfTopicsInBundleCheck
    {
        public static async Task<NamespaceBundleConfigurations> Run(IManageNamespaceManagerLifeCycleInternal manageNamespaceManagerLifeCycle, NamespaceConfigurations namespaceConfigurations, string bundlePrefix)
        {
            var namespaceBundleConfigurations = new NamespaceBundleConfigurations();
            var topicInBundleNameRegex = new Regex($@"^{bundlePrefix}\d+$", RegexOptions.CultureInvariant);

            foreach (var namespaceConfiguration in namespaceConfigurations)
            {
                var namespaceManager = manageNamespaceManagerLifeCycle.Get(namespaceConfiguration.Alias);

                var numberOfTopics = 1;
                if (await namespaceManager.CanManageEntities().ConfigureAwait(false))
                {
                    var filter = $"startswith(path, '{bundlePrefix}') eq true";
                    var foundTopics = await namespaceManager.GetTopics(filter).ConfigureAwait(false);
                    numberOfTopics = CountTopicsInBundle(topicInBundleNameRegex, foundTopics);
                }
                namespaceBundleConfigurations.Add(namespaceConfiguration.Alias, numberOfTopics);
            }

            return namespaceBundleConfigurations;
        }

        public static int CountTopicsInBundle(Regex topicInBundleNameRegex, IEnumerable<TopicDescription> topics)
        {
            return topics.Count(topic => topicInBundleNameRegex.IsMatch(topic.Path));
        }
    }
}