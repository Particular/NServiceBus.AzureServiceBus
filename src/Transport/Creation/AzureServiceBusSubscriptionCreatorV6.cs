namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Linq;
    using System.Threading.Tasks;
    using Logging;
    using Microsoft.ServiceBus.Messaging;

    class AzureServiceBusSubscriptionCreatorV6 : ICreateAzureServiceBusSubscriptionsInternal
    {
        public AzureServiceBusSubscriptionCreatorV6(TopologySubscriptionSettings subscriptionSettings)
        {
            creator = new AzureServiceBusSubscriptionCreator(subscriptionSettings);
        }

        public async Task<SubscriptionDescription> Create(string topicPath, string subscriptionName, SubscriptionMetadataInternal metadata, string sqlFilter, INamespaceManagerInternal namespaceManager, string forwardTo = null)
        {
            var subscriptionDescription = await creator.Create(topicPath, subscriptionName, metadata, sqlFilter, namespaceManager, forwardTo).ConfigureAwait(false);

            if (await SubscriptionIsReusedAcrossDifferentNamespaces(subscriptionDescription, sqlFilter, namespaceManager).ConfigureAwait(false))
            {
                logger.Debug("Creating subscription using event type full name");
                subscriptionDescription = await creator.Create(topicPath, metadata.SubscriptionNameBasedOnEventWithNamespace, metadata, sqlFilter, namespaceManager, forwardTo).ConfigureAwait(false);
            }

            return subscriptionDescription;
        }

        public async Task DeleteSubscription(string topicPath, string subscriptionName, SubscriptionMetadataInternal metadata, string sqlFilter, INamespaceManagerInternal namespaceManager, string forwardTo)
        {
            var subscriptionDescription = new SubscriptionDescription(topicPath, subscriptionName);

            // Check subscription with event name only is the one we should delete. If it's reused, then we need to use event full name.
            if (await SubscriptionIsReusedAcrossDifferentNamespaces(subscriptionDescription, sqlFilter, namespaceManager).ConfigureAwait(false))
            {
                logger.Debug("Deleting subscription using event type full name");
                subscriptionDescription = new SubscriptionDescription(topicPath, metadata.SubscriptionNameBasedOnEventWithNamespace);
            }

            // delete subscription based on event name only
            await creator.DeleteSubscription(subscriptionDescription.TopicPath, subscriptionDescription.Name, metadata, sqlFilter, namespaceManager, forwardTo).ConfigureAwait(false);
        }

        async Task<bool> SubscriptionIsReusedAcrossDifferentNamespaces(SubscriptionDescription subscriptionDescription, string sqlFilter, INamespaceManagerInternal namespaceManager)
        {
            var foundRules = await namespaceManager.GetRules(subscriptionDescription).ConfigureAwait(false);
            var rules = foundRules as RuleDescription[] ?? foundRules.ToArray();

            if (rules.First().Filter is SqlFilter filter)
            {
                if (!filter.SqlExpression.Contains(sqlFilter))
                {
                    logger.Debug("Looks like this subscription name is already taken as the sql filter does not match the subscribed event name.");
                    return true;
                }

                if (sqlFilter.Length != filter.SqlExpression.Length && rules.Length == 1)
                {
                    logger.Info($"SQL filter of the existing subscription '{subscriptionDescription.Name}' should be optimized.\nUpdate Rule filter from \"{filter.SqlExpression}\" to \"{sqlFilter}\".");
                }
            }

            return false;
        }

        AzureServiceBusSubscriptionCreator creator;
        ILog logger = LogManager.GetLogger<AzureServiceBusSubscriptionCreatorV6>();
    }
}