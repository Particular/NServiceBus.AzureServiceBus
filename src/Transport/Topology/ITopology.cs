namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Routing;
    using Settings;
    using Transport;

    static class ObsoleteMessages
    {
        public const string WillBeInternalized = "Internal contract that shouldn't be exposed.";
        public const string NumberOfTopicsInTheBundleWillBeRemoved = "Number of topics in the bundle by default is 2. This setting will be removed in the next major version and number of topics used will be 1.";
    }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface ITopology
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
    }
}