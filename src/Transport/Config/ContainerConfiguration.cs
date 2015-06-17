namespace NServiceBus.Features
{
    using System;
    using Azure.Transports.WindowsAzureServiceBus;
    using Config;
    using Transports;

    class ContainerConfiguration
    {
        public void Configure(FeatureConfigurationContext context, AzureServiceBusQueueConfig configSection)
        {
            ConfigureCreationInfrastructure(context, configSection);

            ConfigureReceiveInfrastructure(context, configSection);

            if (!context.Container.HasComponent<ISendMessages>())
            {
                ConfigureSendInfrastructure(context, configSection);
            }

            if (!context.Container.HasComponent<IPublishMessages>() &&
                !context.Container.HasComponent<IManageSubscriptions>())
            {
                ConfigurePublishingInfrastructure(context, configSection);
            }
        }

        void ConfigureReceiveInfrastructure(FeatureConfigurationContext context, AzureServiceBusQueueConfig configSection)
        {
            context.Container.ConfigureComponent<AzureServiceBusDequeueStrategy>(DependencyLifecycle.InstancePerCall);
            context.Container.ConfigureProperty<AzureServiceBusDequeueStrategy>(t => t.BatchSize, configSection.BatchSize);

            context.Container.ConfigureComponent<AzureServiceBusQueueNotifier>(DependencyLifecycle.InstancePerCall);
            context.Container.ConfigureProperty<AzureServiceBusQueueNotifier>(t => t.ServerWaitTime, configSection.ServerWaitTime);
            context.Container.ConfigureProperty<AzureServiceBusQueueNotifier>(t => t.BatchSize, configSection.BatchSize);
            context.Container.ConfigureProperty<AzureServiceBusQueueNotifier>(t => t.BackoffTimeInSeconds, configSection.BackoffTimeInSeconds);

            context.Container.ConfigureComponent<AzureServiceBusSubscriptionNotifier>(DependencyLifecycle.InstancePerCall);
            context.Container.ConfigureProperty<AzureServiceBusSubscriptionNotifier>(t => t.ServerWaitTime, configSection.ServerWaitTime);
            context.Container.ConfigureProperty<AzureServiceBusSubscriptionNotifier>(t => t.BatchSize, configSection.BatchSize);
            context.Container.ConfigureProperty<AzureServiceBusSubscriptionNotifier>(t => t.BackoffTimeInSeconds, configSection.BackoffTimeInSeconds);
        }

        void ConfigurePublishingInfrastructure(FeatureConfigurationContext context, AzureServiceBusQueueConfig configSection)
        {
            context.Container.ConfigureComponent<AzureServiceBusPublisher>(DependencyLifecycle.InstancePerCall);
            context.Container.ConfigureComponent<AzureServiceBusTopicPublisher>(DependencyLifecycle.InstancePerCall);
            context.Container.ConfigureProperty<AzureServiceBusTopicPublisher>(t => t.MaxDeliveryCount, configSection.MaxDeliveryCount);

            context.Container.ConfigureComponent<AzureServiceBusTopicSubscriptionManager>(DependencyLifecycle.InstancePerCall);
        }

        void ConfigureSendInfrastructure(FeatureConfigurationContext context, AzureServiceBusQueueConfig configSection)
        {
            context.Container.ConfigureComponent<AzureServiceBusSender>(DependencyLifecycle.InstancePerCall);
            context.Container.ConfigureComponent<AzureServiceBusQueueSender>(DependencyLifecycle.InstancePerCall);
            context.Container.ConfigureProperty<AzureServiceBusQueueSender>(t => t.MaxDeliveryCount, configSection.MaxDeliveryCount);
        }

        void ConfigureCreationInfrastructure(FeatureConfigurationContext context, AzureServiceBusQueueConfig configSection)
        {
            context.Container.ConfigureComponent<ManageMessagingFactoriesLifeCycle>(DependencyLifecycle.SingleInstance);
            context.Container.ConfigureComponent<CreatesMessagingFactories>(DependencyLifecycle.SingleInstance);
            context.Container.ConfigureComponent<CreatesNamespaceManagers>(DependencyLifecycle.SingleInstance);

            context.Container.ConfigureComponent<ManageQueueClientsLifeCycle>(DependencyLifecycle.SingleInstance);
            context.Container.ConfigureComponent<AzureServicebusQueueClientCreator>(DependencyLifecycle.InstancePerCall);
            context.Container.ConfigureProperty<AzureServicebusQueueClientCreator>(t => t.BatchSize, configSection.BatchSize);

            context.Container.ConfigureComponent<AzureServiceBusTopologyCreator>(DependencyLifecycle.InstancePerCall);

            context.Container.ConfigureComponent<AzureServiceBusQueueCreator>(DependencyLifecycle.InstancePerCall);
            context.Container.ConfigureProperty<AzureServiceBusQueueCreator>(t => t.LockDuration, TimeSpan.FromMilliseconds(configSection.LockDuration));
            context.Container.ConfigureProperty<AzureServiceBusQueueCreator>(t => t.MaxSizeInMegabytes, configSection.MaxSizeInMegabytes);
            context.Container.ConfigureProperty<AzureServiceBusQueueCreator>(t => t.RequiresDuplicateDetection, configSection.RequiresDuplicateDetection);
            context.Container.ConfigureProperty<AzureServiceBusQueueCreator>(t => t.RequiresSession, configSection.RequiresSession);
            context.Container.ConfigureProperty<AzureServiceBusQueueCreator>(t => t.DefaultMessageTimeToLive, TimeSpan.FromMilliseconds(configSection.DefaultMessageTimeToLive));
            context.Container.ConfigureProperty<AzureServiceBusQueueCreator>(t => t.EnableDeadLetteringOnMessageExpiration, configSection.EnableDeadLetteringOnMessageExpiration);
            context.Container.ConfigureProperty<AzureServiceBusQueueCreator>(t => t.DuplicateDetectionHistoryTimeWindow, TimeSpan.FromMilliseconds(configSection.DuplicateDetectionHistoryTimeWindow));
            context.Container.ConfigureProperty<AzureServiceBusQueueCreator>(t => t.MaxDeliveryCount, configSection.MaxDeliveryCount);
            context.Container.ConfigureProperty<AzureServiceBusQueueCreator>(t => t.EnableBatchedOperations, configSection.EnableBatchedOperations);
            context.Container.ConfigureProperty<AzureServiceBusQueueCreator>(t => t.EnablePartitioning, configSection.EnablePartitioning);
            context.Container.ConfigureProperty<AzureServiceBusQueueCreator>(t => t.SupportOrdering, configSection.SupportOrdering);

            context.Container.ConfigureComponent<ManageTopicClientsLifeCycle>(DependencyLifecycle.SingleInstance);
            context.Container.ConfigureComponent<AzureServicebusTopicClientCreator>(DependencyLifecycle.InstancePerCall);
            context.Container.ConfigureComponent<AzureServiceBusTopicCreator>(DependencyLifecycle.InstancePerCall);
            context.Container.ConfigureProperty<AzureServiceBusTopicCreator>(t => t.LockDuration, TimeSpan.FromMilliseconds(configSection.LockDuration));
            context.Container.ConfigureProperty<AzureServiceBusTopicCreator>(t => t.MaxSizeInMegabytes, configSection.MaxSizeInMegabytes);
            context.Container.ConfigureProperty<AzureServiceBusTopicCreator>(t => t.RequiresDuplicateDetection, configSection.RequiresDuplicateDetection);
            context.Container.ConfigureProperty<AzureServiceBusTopicCreator>(t => t.RequiresSession, configSection.RequiresSession);
            context.Container.ConfigureProperty<AzureServiceBusTopicCreator>(t => t.DefaultMessageTimeToLive, TimeSpan.FromMilliseconds(configSection.DefaultMessageTimeToLive));
            context.Container.ConfigureProperty<AzureServiceBusTopicCreator>(t => t.EnableDeadLetteringOnMessageExpiration, configSection.EnableDeadLetteringOnMessageExpiration);
            context.Container.ConfigureProperty<AzureServiceBusTopicCreator>(t => t.DuplicateDetectionHistoryTimeWindow, TimeSpan.FromMilliseconds(configSection.DuplicateDetectionHistoryTimeWindow));
            context.Container.ConfigureProperty<AzureServiceBusTopicCreator>(t => t.MaxDeliveryCount, configSection.MaxDeliveryCount);
            context.Container.ConfigureProperty<AzureServiceBusTopicCreator>(t => t.EnableBatchedOperations, configSection.EnableBatchedOperations);
            context.Container.ConfigureProperty<AzureServiceBusTopicCreator>(t => t.EnablePartitioning, configSection.EnablePartitioning);
            context.Container.ConfigureProperty<AzureServiceBusTopicCreator>(t => t.SupportOrdering, configSection.SupportOrdering);

            context.Container.ConfigureComponent<AzureServicebusSubscriptionClientCreator>(DependencyLifecycle.InstancePerCall);
            context.Container.ConfigureProperty<AzureServicebusSubscriptionClientCreator>(t => t.BatchSize, configSection.BatchSize);

            context.Container.ConfigureComponent<AzureServiceBusSubscriptionCreator>(DependencyLifecycle.InstancePerCall);
            context.Container.ConfigureProperty<AzureServiceBusSubscriptionCreator>(t => t.LockDuration, TimeSpan.FromMilliseconds(configSection.LockDuration));
            context.Container.ConfigureProperty<AzureServiceBusSubscriptionCreator>(t => t.RequiresSession, configSection.RequiresSession);
            context.Container.ConfigureProperty<AzureServiceBusSubscriptionCreator>(t => t.DefaultMessageTimeToLive, TimeSpan.FromMilliseconds(configSection.DefaultMessageTimeToLive));
            context.Container.ConfigureProperty<AzureServiceBusSubscriptionCreator>(t => t.EnableDeadLetteringOnMessageExpiration, configSection.EnableDeadLetteringOnMessageExpiration);
            context.Container.ConfigureProperty<AzureServiceBusSubscriptionCreator>(t => t.EnableDeadLetteringOnFilterEvaluationExceptions, configSection.EnableDeadLetteringOnFilterEvaluationExceptions);
            context.Container.ConfigureProperty<AzureServiceBusSubscriptionCreator>(t => t.MaxDeliveryCount, configSection.MaxDeliveryCount);
            context.Container.ConfigureProperty<AzureServiceBusSubscriptionCreator>(t => t.EnableBatchedOperations, configSection.EnableBatchedOperations);
        }

    }
}