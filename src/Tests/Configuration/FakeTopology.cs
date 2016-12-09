namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using System;
    using System.Threading.Tasks;
    using Routing;
    using Settings;
    using Transport;
    using Transport.AzureServiceBus;

    class FakeTopology : ITopologyInternal
    {
        public FakeTopology(SettingsHolder settings)
        {
            settings.Set<ITopologyInternal>(this);
        }

        public TopologySettings Settings { get; } = new TopologySettings();

        public void Initialize(SettingsHolder settings)
        {
            throw new NotImplementedException();
        }

        public EndpointInstance BindToLocalEndpoint(EndpointInstance instance)
        {
            throw new NotImplementedException();
        }

        public Func<ICreateQueues> GetQueueCreatorFactory()
        {
            throw new NotImplementedException();
        }

        public Func<IPushMessages> GetMessagePumpFactory()
        {
            throw new NotImplementedException();
        }

        public Func<IDispatchMessages> GetDispatcherFactory()
        {
            throw new NotImplementedException();
        }

        public Task<StartupCheckResult> RunPreStartupChecks()
        {
            throw new NotImplementedException();
        }

        public Func<IManageSubscriptions> GetSubscriptionManagerFactory()
        {
            throw new NotImplementedException();
        }

        public OutboundRoutingPolicy GetOutboundRoutingPolicy()
        {
            throw new NotImplementedException();
        }

        public bool HasNativePubSubSupport { get; }
        public bool HasSupportForCentralizedPubSub { get; }

        public Task Stop()
        {
            throw new NotImplementedException();
        }
    }
}