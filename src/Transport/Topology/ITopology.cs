namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Threading.Tasks;
    using Routing;
    using Settings;
    using Transport;

    [ObsoleteEx(Message = "Internal contract that shouldn't be exposed.", TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
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