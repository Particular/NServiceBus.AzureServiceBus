namespace NServiceBus.AzureServiceBus
{
    using System;
    using NServiceBus.Settings;
    using NServiceBus.Transports;

    public interface ITopology {

        void Initialize(SettingsHolder settings);

        Func<ICreateQueues> GetQueueCreatorFactory();
        Func<CriticalError, IPushMessages> GetMessagePumpFactory();
        Func<IDispatchMessages> GetDispatcherFactory();
        IManageSubscriptions GetSubscriptionManager();
        OutboundRoutingPolicy GetOutboundRoutingPolicy();
    }
}