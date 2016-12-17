namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using Logging;
    using Settings;

    class AzureServiceBusSubscriptionCreatorV6 : ICreateAzureServiceBusSubscriptionsInternal
    {
        AzureServiceBusSubscriptionCreator creator;
        ILog logger = LogManager.GetLogger<AzureServiceBusSubscriptionCreatorV6>();

        public AzureServiceBusSubscriptionCreatorV6(TopologySubscriptionSettings subscriptionSettings, ReadOnlySettings settings)
        {
            creator = new AzureServiceBusSubscriptionCreator(subscriptionSettings, settings);
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

            // check the if the subscription with event name only is the one we should delete, i.e. event with another namespace owns it
            if (!await SubscriptionIsReusedAcrossDifferentNamespaces(subscriptionDescription, sqlFilter, namespaceManager).ConfigureAwait(false))
            {
                logger.Debug("Deleting subscription using event type full name");
                subscriptionDescription = new SubscriptionDescription(topicPath, metadata.SubscriptionNameBasedOnEventWithNamespace);
            }

            // delete subscription based on event name only
            await namespaceManager.DeleteSubscription(subscriptionDescription).ConfigureAwait(false);
        }

        async Task<bool> SubscriptionIsReusedAcrossDifferentNamespaces(SubscriptionDescription subscriptionDescription, string sqlFilter, INamespaceManagerInternal namespaceManager)
        {
            var rules = await namespaceManager.GetRules(subscriptionDescription).ConfigureAwait(false);
            var filter = rules.First().Filter as SqlFilter;
            if (filter != null && filter.SqlExpression != sqlFilter)
            {
                logger.Debug("Looks like this subscription name is already taken as the sql filter does not match the subscribed event name.");
                return true;
            }

            return false;
        }
    }
}