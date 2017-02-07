namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology.Operation
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
    using NUnit.Framework;
    using Transport;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_operating_EndpointOrientedTopology
    {
        [Test]
        public async Task Receives_incoming_messages_from_endpoint_queue()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            // cleanup
            await TestUtility.Delete("sales");

            // setting up the environment
            var container = new TransportPartsContainer();
            var settings = new SettingsHolder();

            var topology = await SetupEndpointOrientedTopology(settings, container, "sales");

            // setup the operator
            var topologyOperator = (IOperateTopologyInternal)container.Resolve(typeof(TopologyOperator));

            var completed = new AsyncManualResetEvent(false);
            var error = new AsyncManualResetEvent(false);

            var received = false;
            Exception ex = null;

            topologyOperator.OnIncomingMessage((message, context) =>
            {
                received = true;

                completed.Set();

                return TaskEx.Completed;
            });
            topologyOperator.OnError(exception =>
            {
                ex = exception;

                error.Set();

                return TaskEx.Completed;
            });

            // execute
            topologyOperator.Start(topology.DetermineReceiveResources("sales"), 1);

            // send message to queue
            var senderFactory = new MessageSenderCreator(new MessagingFactoryLifeCycleManager(new MessagingFactoryCreator(new NamespaceManagerLifeCycleManagerInternal(new NamespaceManagerCreator(settings)), settings), settings), settings);
            var sender = await senderFactory.Create("sales", null, "namespace");
            await sender.Send(new BrokeredMessage());

            await Task.WhenAny(completed.WaitAsync(cts.Token).IgnoreCancellation(), error.WaitAsync(cts.Token).IgnoreCancellation());


            // validate
            Assert.IsTrue(received);
            Assert.IsNull(ex);

            await topologyOperator.Stop();
        }

        [Test]
        public async Task Calls_on_error_when_error_during_processing()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            // cleanup
            await TestUtility.Delete("sales");

            // setting up the environment
            var container = new TransportPartsContainer();
            var settings = new SettingsHolder();

            var topology = await SetupEndpointOrientedTopology(settings, container, "sales");

            // setup the operator
            var topologyOperator = (IOperateTopologyInternal)container.Resolve(typeof(TopologyOperator));

            var error = new AsyncManualResetEvent(false);

            var received = false;
            var errorOccured = false;

            topologyOperator.OnIncomingMessage(async (message, context) =>
            {
                received = true;
                await Task.Delay(1, cts.Token).ConfigureAwait(false);
                throw new Exception("Something went wrong");
            });
            topologyOperator.OnError(exception =>
            {
                errorOccured = true;

                error.Set();

                return TaskEx.Completed;
            });

            // execute
            topologyOperator.Start(topology.DetermineReceiveResources("sales"), 1);

            // send message to queue
            var senderFactory = new MessageSenderCreator(new MessagingFactoryLifeCycleManager(new MessagingFactoryCreator(new NamespaceManagerLifeCycleManagerInternal(new NamespaceManagerCreator(settings)), settings), settings), settings);
            var sender = await senderFactory.Create("sales", null, "namespace");
            await sender.Send(new BrokeredMessage());

            await error.WaitAsync(cts.Token);

            // validate
            Assert.IsTrue(received);
            Assert.IsTrue(errorOccured);

            await topologyOperator.Stop();
        }


        [Test]
        public async Task Completes_incoming_message_when_successfully_received()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            // cleanup
            await TestUtility.Delete("sales");

            // setting up the environment
            var container = new TransportPartsContainer();
            var settings = new SettingsHolder();

            var topology = await SetupEndpointOrientedTopology(settings, container, "sales");

            // setup the operator
            var topologyOperator = (IOperateTopologyInternal)container.Resolve(typeof(TopologyOperator));

            var completed = new AsyncManualResetEvent(false);
            var error = new AsyncManualResetEvent(false);

            var received = false;
            Exception ex = null;

            topologyOperator.OnIncomingMessage((message, context) =>
            {
                received = true;

                completed.Set();

                return TaskEx.Completed;
            });
            topologyOperator.OnError(exception =>
            {
                ex = exception;

                error.Set();

                return TaskEx.Completed;
            });

            // execute
            topologyOperator.Start(topology.DetermineReceiveResources("sales"), 1);

            // send message to queue
            var senderFactory = new MessageSenderCreator(new MessagingFactoryLifeCycleManager(new MessagingFactoryCreator(new NamespaceManagerLifeCycleManagerInternal(new NamespaceManagerCreator(settings)), settings), settings), settings);
            var sender = await senderFactory.Create("sales", null, "namespace");
            await sender.Send(new BrokeredMessage());

            await Task.WhenAny(completed.WaitAsync(cts.Token).IgnoreCancellation(), error.WaitAsync(cts.Token).IgnoreCancellation());

            // validate
            Assert.IsTrue(received);
            Assert.IsNull(ex);

            await Task.Delay(TimeSpan.FromSeconds(5), cts.Token); // give asb some time to update stats

            var namespaceLifeCycle = new NamespaceManagerLifeCycleManagerInternal(new NamespaceManagerCreator(settings));
            var namespaceManager = namespaceLifeCycle.Get("namespace");
            var queueDescription = await namespaceManager.GetQueue("sales");
            Assert.AreEqual(0, queueDescription.MessageCount);

            await topologyOperator.Stop();
        }

        [Test]
        public async Task Aborts_incoming_message_when_error_during_processing()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            // cleanup
            await TestUtility.Delete("sales");

            // setting up the environment
            var container = new TransportPartsContainer();
            var settings = new SettingsHolder();

            var topology = await SetupEndpointOrientedTopology(settings, container, "sales");

            // setup the operator
            var topologyOperator = (IOperateTopologyInternal)container.Resolve(typeof(TopologyOperator));

            var completed = new AsyncManualResetEvent(false);
            var error = new AsyncManualResetEvent(false);

            var received = false;
            var errorOccured = false;

            topologyOperator.OnIncomingMessage(async (message, context) =>
            {
                received = true;
                await Task.Delay(1, cts.Token).ConfigureAwait(false);
                throw new Exception("Something went wrong");
            });
            topologyOperator.OnError(exception =>
            {
                errorOccured = true;

                error.Set();

                return TaskEx.Completed;
            });

            // execute
            topologyOperator.Start(topology.DetermineReceiveResources("sales"), 1);

            // send message to queue
            var senderFactory = new MessageSenderCreator(new MessagingFactoryLifeCycleManager(new MessagingFactoryCreator(new NamespaceManagerLifeCycleManagerInternal(new NamespaceManagerCreator(settings)), settings), settings), settings);
            var sender = await senderFactory.Create("sales", null, "namespace");
            await sender.Send(new BrokeredMessage());

            await Task.WhenAny(completed.WaitAsync(cts.Token).IgnoreCancellation(), error.WaitAsync(cts.Token).IgnoreCancellation());

            // validate
            Assert.IsTrue(received);
            Assert.IsTrue(errorOccured);

            await Task.Delay(TimeSpan.FromSeconds(5), cts.Token); // give asb some time to update stats

            var namespaceLifeCycle = new NamespaceManagerLifeCycleManagerInternal(new NamespaceManagerCreator(settings));
            var namespaceManager = namespaceLifeCycle.Get("namespace");
            var queueDescription = await namespaceManager.GetQueue("sales");
            Assert.AreEqual(1, queueDescription.MessageCount);

            await topologyOperator.Stop();
        }

        async Task<ITopologySectionManagerInternal> SetupEndpointOrientedTopology(SettingsHolder settings, TransportPartsContainer container, string enpointname)
        {
            settings.Set<Conventions>(new Conventions());
            container.Register(typeof(SettingsHolder), () => settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            settings.SetDefault("NServiceBus.Routing.EndpointName", enpointname);
            extensions.NamespacePartitioning().AddNamespace("namespace", AzureServiceBusConnectionString.Value);

            var topology = new EndpointOrientedTopologyInternal(container);
            topology.Initialize(settings);

            // create the topologySectionManager
            var topologyCreator = (ICreateTopologyInternal)container.Resolve(typeof(TopologyCreator));

            var sectionManager = container.Resolve<ITopologySectionManagerInternal>();
            await topologyCreator.Create(sectionManager.DetermineResourcesToCreate(new QueueBindings()));
            return sectionManager;
        }
    }
}
