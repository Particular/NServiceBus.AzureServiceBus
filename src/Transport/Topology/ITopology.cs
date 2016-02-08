namespace NServiceBus.AzureServiceBus
{
    using System;
    using NServiceBus.Settings;
    using NServiceBus.Transports;

    public interface ITopology {

        void Initialize(SettingsHolder settings);

        Func<ICreateQueues> GetQueueCreatorFactory();
        // TODO: CriticalError no longer passed in to MessagePumpFactory. Ensure that Core is doing pushMessages.OnCriticalError(error);
        Func<IPushMessages> GetMessagePumpFactory();
        Func<IDispatchMessages> GetDispatcherFactory();
        StartupCheckResult ApplyPreStartupChecks();
        IManageSubscriptions GetSubscriptionManager();
        OutboundRoutingPolicy GetOutboundRoutingPolicy();
        bool HasNativePubSubSupport { get; }
        bool HasSupportForCentralizedPubSub { get;}
    }
}