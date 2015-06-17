namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.QueueAndTopicByEndpoint
{
    using System;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Logging;
    using Settings;
    using Transports;

    /// <summary>
    /// Sends occur through queues, one for each endpoint, 
    /// publishes through a topic per endpoint, 
    /// receives on both it's own queue &amp; subscriptions per datatype
    /// </summary>
    class QueueAndTopicByEndpointTopology : ITopology
    {
        Configure config;
        IManageMessagingFactoriesLifecycle messagingFactories;
        ICreateSubscriptions subscriptionCreator;
        ICreateQueues queueCreator;
        ICreateTopics topicCreator;
        IManageQueueClientsLifecycle queueClients; 
        ICreateSubscriptionClients subscriptionClients;
        IManageTopicClientsLifecycle topicClients;
        ICreateQueueClients queueClientCreator;

        ILog logger = LogManager.GetLogger(typeof(QueueAndTopicByEndpointTopology));

        internal QueueAndTopicByEndpointTopology(
            Configure config, 
            IManageMessagingFactoriesLifecycle messagingFactories,
            ICreateSubscriptions subscriptionCreator, 
            ICreateQueues queueCreator,
            ICreateTopics topicCreator,
            IManageQueueClientsLifecycle queueClients, 
            ICreateSubscriptionClients subscriptionClients,
            IManageTopicClientsLifecycle topicClients, 
            ICreateQueueClients queueClientCreator)
        {
            this.config = config;
            this.messagingFactories = messagingFactories;
            this.subscriptionCreator = subscriptionCreator;
            this.queueCreator = queueCreator;
            this.topicCreator = topicCreator;
            this.queueClients = queueClients;
            this.subscriptionClients = subscriptionClients;
            this.topicClients = topicClients;
            this.queueClientCreator = queueClientCreator;
        }

        public void Initialize(ReadOnlySettings settings)
        {
            
        }

        public INotifyReceivedBrokeredMessages Subscribe(Type eventType, Address address)
        {
            var publisherAddress = NamingConventions.PublisherAddressConventionForSubscriptions(config.Settings, address);
            var notifier = config.Builder.Build<AzureServiceBusSubscriptionNotifier>();
            notifier.SubscriptionClient = CreateSubscriptionClient(eventType, publisherAddress);
            notifier.MessageType = eventType;
            notifier.Address = publisherAddress;
            return notifier;
        }

        SubscriptionClient CreateSubscriptionClient(Type eventType, Address address)
        {
            var subscriptionname = NamingConventions.SubscriptionNamingConvention(config.Settings, eventType, config.Settings.EndpointName());
            var factory = messagingFactories.Get(address);

            try
            {
                var description = subscriptionCreator.Create(address, eventType, subscriptionname);
                return subscriptionClients.Create(description, factory);
            }
            catch (SubscriptionAlreadyInUseException)
            {
                // if this occurs, it means that another endpoint is using the same eventtype name but in another namespace,
                // so let's differenatiate including this namespace, odds are very likely that we will get a guid instead
                // that's why we're not defaulting to this convention.

                subscriptionname = NamingConventions.SubscriptionFullNamingConvention(config.Settings, eventType, config.Settings.EndpointName());
                var description = subscriptionCreator.Create(address, eventType, subscriptionname);
                return subscriptionClients.Create(description, factory);
            }

        }

        public void Unsubscribe(INotifyReceivedBrokeredMessages notifier)
        {
            var subscriptionname = NamingConventions.SubscriptionNamingConvention(config.Settings, notifier.MessageType, config.Settings.EndpointName());

            subscriptionCreator.Delete(notifier.Address, subscriptionname);
        }

        public INotifyReceivedBrokeredMessages GetReceiver(Address original)
        {
            var address = NamingConventions.QueueAddressConvention(config.Settings, original, false);
            var desc = queueCreator.Create(address); //we shouldn't do this over and over
            var factory = messagingFactories.Get(address);
            var notifier = (AzureServiceBusQueueNotifier)config.Builder.Build(typeof(AzureServiceBusQueueNotifier));
            notifier.QueueClient = queueClientCreator.Create(desc, factory);

            //todo: notifier.BatchSize = maximumConcurrencyLevel;
            return notifier;
        }

        public ISendBrokeredMessages GetSender(Address original)
        {
            var address = NamingConventions.QueueAddressConvention(config.Settings, original, true);
            queueCreator.Create(address); //we shouldn't do this over and over
            var sender = (AzureServiceBusQueueSender)config.Builder.Build(typeof(AzureServiceBusQueueSender));
            sender.QueueClient = queueClients.Get(address);
            return sender;
        }

        public IPublishBrokeredMessages GetPublisher(Address original)
        {
            var address = NamingConventions.PublisherAddressConvention(config.Settings, original);
            topicCreator.Create(address); //we shouldn't do this over and over
            var publisher = (AzureServiceBusTopicPublisher)config.Builder.Build(typeof(AzureServiceBusTopicPublisher));
            publisher.TopicClient = topicClients.Get(address);
            return publisher;
        }

        public void Create(Address original)
        {
            logger.InfoFormat("Going to create queue for address '{0}' if needed", original.Queue);

            var queue = NamingConventions.QueueAddressConvention(config.Settings, original, false);
            queueCreator.Create(queue);

            logger.InfoFormat("Going to create topic for address '{0}' if needed", original.Queue);
            if (original == config.LocalAddress)
            {
                var topic = NamingConventions.PublisherAddressConvention(config.Settings, original);
                topicCreator.Create(topic);
            }
            else
            {
                logger.InfoFormat("Did not create topic for address  '{0}' as it does not correspond to the local address", original.Queue);
            }
        }
    }
}