namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Routing;
    using Settings;
    using Transport;

    static class ObsoleteMessages
    {
        public const string InternalizedContract = "Internal contract.";
    }

    interface ITopologyInternal
    {
        void Initialize(SettingsHolder settings);
        EndpointInstance BindToLocalEndpoint(EndpointInstance instance);
        Func<ICreateQueues> GetQueueCreatorFactory();
        Func<IPushMessages> GetMessagePumpFactory();
        Func<IDispatchMessages> GetDispatcherFactory();
        Task<StartupCheckResult> RunPreStartupChecks();
        Func<IManageSubscriptions> GetSubscriptionManagerFactory();
        OutboundRoutingPolicy GetOutboundRoutingPolicy();

        bool HasNativePubSubSupport { get; }
        bool HasSupportForCentralizedPubSub { get;}

        Task Stop();
    }
}