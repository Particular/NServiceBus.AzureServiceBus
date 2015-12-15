namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Seam
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.AzureServiceBus.Tests;
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
            var container = new TransportPartsContainer();

            var topology = await SetupBasicTopology(container, "sales");

            // setup the operator
            var topologyOperator = (IOperateTopology)container.Resolve(typeof(TopologyOperator));

            var pump = new MessagePump(topology, topologyOperator);

            var completed = new AsyncAutoResetEvent(false);
            //var error = new AsyncAutoResetEvent(false);

            var received = false;
            Exception ex = null;

            // Dummy CriticalError
            var criticalError = new CriticalError((endpoint, error, exception) => Task.FromResult(0));

            await pump.Init(context =>
            {
                received = true;

                completed.Set();

                return Task.FromResult(true);

                // TODO: TransportTransactionMode will need to change with topology
            }, criticalError, new PushSettings("sales", "error", false, TransportTransactionMode.ReceiveOnly));


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
            var senderFactory = (MessageSenderCreator)container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create("sales", null, AzureServiceBusConnectionString.Value);
            await sender.Send(new BrokeredMessage());

            await completed.WaitOne(); // Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(received);
            Assert.IsNull(ex);

            // cleanup 
            await pump.Stop();

            await Cleanup(container, "sales");
        }

        static async Task Cleanup(TransportPartsContainer container, string enpointname)
        {
            var namespaceLifeCycle = (IManageNamespaceManagerLifeCycle)container.Resolve(typeof(IManageNamespaceManagerLifeCycle));
            var namespaceManager = namespaceLifeCycle.Get(AzureServiceBusConnectionString.Value);
            await namespaceManager.DeleteQueue(enpointname);
        }

        async Task<ITopologySectionManager> SetupBasicTopology(TransportPartsContainer container, string enpointname)
        {
            var settings = new SettingsHolder();
            container.Register(typeof(SettingsHolder), () => settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            settings.SetDefault<Endpoint>(new Endpoint(enpointname));
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace(AzureServiceBusConnectionString.Value);

            var topology = new BasicTopology(container);

            topology.Initialize(settings);

            // create the topologySectionManager
            var topologyCreator = (ICreateTopology)container.Resolve(typeof(TopologyCreator));
            var sectionManager = container.Resolve<ITopologySectionManager>();
            await topologyCreator.Create(sectionManager.DetermineResourcesToCreate());
            return sectionManager;
        }
    }
}