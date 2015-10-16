namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Seam
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.AzureServiceBus.Tests;
    using NServiceBus.ConsistencyGuarantees;
    using NServiceBus.Settings;
    using NServiceBus.Transports;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_receiving_messages
    {
        [Test]
        public async Task Pushes_received_message_into_pipeline()
        {
            // setting up the environment
            var container = new FuncBuilder();

            var topology = await SetupBasicTopology(container, "sales");

            // setup the operator
            var topologyOperator = (IOperateTopology)container.Build(typeof(TopologyOperator));

            var pump = new MessagePump(topology, topologyOperator);

            var completed = new AsyncAutoResetEvent(false);
            //var error = new AsyncAutoResetEvent(false);

            var received = false;
            Exception ex = null;

            pump.Init(context =>
            {
                received = true;

                completed.Set();

                return Task.FromResult(true);
            }, new PushSettings("sales", "error", false, ConsistencyGuarantee.AtLeastOnce));


            // how to propagate error's to the core, or should they be handled by us?
            //pump.OnError(exception =>
            //{
            //    ex = exception;

            //    error.Set();

            //    return Task.FromResult(true);
            //});

            // execute
            pump.Start(new PushRuntimeSettings(1));

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Build(typeof(MessageSenderCreator));
            var sender = await senderFactory.CreateAsync("sales", null, AzureServiceBusConnectionString.Value);
            await sender.SendAsync(new BrokeredMessage());

            await completed.WaitOne(); // Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(received);
            Assert.IsNull(ex);

            // cleanup 
            await pump.Stop();

            await Cleanup(container, "sales");
        }

        static async Task Cleanup(FuncBuilder container, string enpointname)
        {
            var namespaceLifeCycle = (IManageNamespaceManagerLifeCycle)container.Build(typeof(IManageNamespaceManagerLifeCycle));
            var namespaceManager = namespaceLifeCycle.Get(AzureServiceBusConnectionString.Value);
            await namespaceManager.DeleteQueueAsync(enpointname);
        }

        async Task<BasicTopology> SetupBasicTopology(FuncBuilder container, string enpointname)
        {
            var settings = new SettingsHolder();
            container.Register(typeof(SettingsHolder), () => settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            settings.SetDefault<EndpointName>(new EndpointName(enpointname));
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(AzureServiceBusConnectionString.Value);

            var topology = new BasicTopology(settings, container);

            topology.InitializeSettings();
            topology.InitializeContainer();

            // create the topology
            var topologyCreator = (ICreateTopology)container.Build(typeof(TopologyCreator));
            await topologyCreator.Create(topology.Determine(Purpose.Creating));
            return topology;
        }
    }
}