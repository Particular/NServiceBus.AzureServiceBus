namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Settings;
    using Transports;

    public interface ITopology
    {
        void Initialize(SettingsHolder settings);

        Func<ICreateQueues> GetQueueCreatorFactory();
        Func<IPushMessages> GetMessagePumpFactory();
        Func<IDispatchMessages> GetDispatcherFactory();
        Task<StartupCheckResult> RunPreStartupChecks();
        Func<IManageSubscriptions> GetSubscriptionManagerFactory();
        OutboundRoutingPolicy GetOutboundRoutingPolicy();

        bool HasNativePubSubSupport { get; }
        bool HasSupportForCentralizedPubSub { get;}
        bool NeedsMappingConfigurationBetweenPublishersAndEventTypes { get; }
    }
}