namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.Settings;
    using NServiceBus.Transports;

    public interface ITopology {

        void Initialize(SettingsHolder settings);

        Func<ICreateQueues> GetQueueCreatorFactory();
        // TODO: CriticalError no longer passed in to MessagePumpFactory. Ensure that Core is doing pushMessages.OnCriticalError(error);
        Func<IPushMessages> GetMessagePumpFactory();
        Func<IDispatchMessages> GetDispatcherFactory();
        Task<StartupCheckResult> RunPreStartupChecks();
        Func<IManageSubscriptions> GetSubscriptionManagerFactory();
        OutboundRoutingPolicy GetOutboundRoutingPolicy();
        bool HasNativePubSubSupport { get; }
        bool HasSupportForCentralizedPubSub { get;}
    }
}