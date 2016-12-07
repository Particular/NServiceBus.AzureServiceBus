namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Seam
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AzureServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Tests;
    using TestUtils;
    using Transport.AzureServiceBus;
    using Settings;
    using Transport;
    using NUnit.Framework;

#pragma warning disable 618
    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_receiving_messages
    {
        [Test]
        public async Task Pushes_received_message_into_pipeline()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            await TestUtility.Delete("sales");

            var settings = new SettingsHolder();
            new DefaultConfigurationValues().Apply(settings);

            // setting up the environment
            var container = new TransportPartsContainer();

            var topology = await SetupEndpointOrientedTopology(container, "sales");

            // setup the operator
            var pump = new MessagePump(topology, container, settings);

            var completed = new AsyncAutoResetEvent(false);
            //var error = new AsyncAutoResetEvent(false);

            var received = false;
            Exception ex = null;

            // Dummy CriticalError
            var criticalError = new CriticalError(ctx => TaskEx.Completed);

            await pump.Init(context =>
            {
                received = true;

                completed.Set();

                return TaskEx.Completed;

            }, null, criticalError, new PushSettings("sales", "error", false, TransportTransactionMode.ReceiveOnly));

            // execute
            pump.Start(new PushRuntimeSettings(1));

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create("sales", null, "namespaceName");
            await sender.Send(new BrokeredMessage());

            await completed.WaitAsync(cts.Token).IgnoreCancellation(); // Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(received);
            Assert.IsNull(ex);

            // cleanup
            await pump.Stop();
        }

        async Task<ITopologySectionManagerInternal> SetupEndpointOrientedTopology(TransportPartsContainer container, string enpointname)
        {
            var settings = new SettingsHolder();
            settings.Set<Conventions>(new Conventions());
            container.Register(typeof(SettingsHolder), () => settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            settings.SetDefault("NServiceBus.Routing.EndpointName", enpointname);
            extensions.NamespacePartitioning().AddNamespace("namespaceName", AzureServiceBusConnectionString.Value);

            var topology = new EndpointOrientedTopologyInternal(container);

            topology.Initialize(settings);

            // create the topologySectionManager
            var topologyCreator = (ICreateTopologyInternal)container.Resolve(typeof(TopologyCreator));
            var sectionManager = container.Resolve<ITopologySectionManagerInternal>();
            await topologyCreator.Create(sectionManager.DetermineResourcesToCreate(new QueueBindings()));
            container.RegisterSingleton<TopologyOperator>();
            return sectionManager;
        }
    }
}