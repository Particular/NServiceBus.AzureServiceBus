namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using Azure.WindowsAzureServiceBus.Tests;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_operating_BasicTopology
    {
        [Test]
        public async Task Receives_incoming_messages_from_endpoint_queue()
        {
            // setting up the environment
            var container = new TransportPartsContainer();

            var topology = await SetupBasicTopology(container, "sales");

            // setup the operator
            var topologyOperator = (IOperateTopology)container.Resolve(typeof(TopologyOperator));

            var completed = new AsyncAutoResetEvent(false);
            var error = new AsyncAutoResetEvent(false);

            var received = false;
            Exception ex = null;

            topologyOperator.OnIncomingMessage((message, context) =>
            {
                received = true;

                completed.Set();

                return Task.FromResult(true);
            });
            topologyOperator.OnError(exception =>
            {
                ex = exception;

                error.Set();

                return Task.FromResult(true);
            });

            // execute
            topologyOperator.Start(topology.DetermineReceiveResources("sales"), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create("sales", null, AzureServiceBusConnectionString.Value);
            await sender.Send(new BrokeredMessage());

            await Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(received);
            Assert.IsNull(ex);

            // cleanup 
            await topologyOperator.Stop();

            await Cleanup(container, "sales");
        }

        [Test]
        public async Task Calls_completion_callbacks_before_completing()
        {
            // setting up the environment
            var container = new TransportPartsContainer();

            var topology = await SetupBasicTopology(container, "sales");

            // setup the operator
            var topologyOperator = (IOperateTopology)container.Resolve(typeof(TopologyOperator));

            var completed = new AsyncAutoResetEvent(false);
            var error = new AsyncAutoResetEvent(false);

            var received = false;
            var completeCalled = false;
            Exception ex = null;

            topologyOperator.OnIncomingMessage((message, context) =>
            {
                received = true;

                var ctx = (BrokeredMessageReceiveContext) context;

                ctx.OnComplete.Add(() =>
                {
                    completeCalled = true;

                    completed.Set();

                    return Task.FromResult(true);
                });

                return Task.FromResult(true);
            });
            topologyOperator.OnError(exception =>
            {
                ex = exception;

                error.Set();

                return Task.FromResult(true);
            });

            // execute
            topologyOperator.Start(topology.DetermineReceiveResources("sales"), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create("sales", null, AzureServiceBusConnectionString.Value);
            await sender.Send(new BrokeredMessage());

            await Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(completeCalled);
            Assert.IsTrue(received);
            Assert.IsNull(ex);

            // cleanup 
            await topologyOperator.Stop();

            await Cleanup(container, "sales");
        }

        [Test]
        public async Task Does_not_call_completion_when_error_during_processing()
        {
            // setting up the environment
            var container = new TransportPartsContainer();

            var topology = await SetupBasicTopology(container, "sales");

            // setup the operator
            var topologyOperator = (IOperateTopology)container.Resolve(typeof(TopologyOperator));

            var completed = new AsyncAutoResetEvent(false);
            var error = new AsyncAutoResetEvent(false);

            var received = false;
            var completeCalled = false;

            topologyOperator.OnIncomingMessage(async (message, context) =>
            {
                received = true;

                var ctx = (BrokeredMessageReceiveContext)context;

                ctx.OnComplete.Add(() =>
                {
                    completeCalled = true;

                    completed.Set();

                    return Task.FromResult(true);
                });

                await Task.Delay(1).ConfigureAwait(false);
                throw new Exception("Something went wrong");
            });
            topologyOperator.OnError(exception =>
            {
                error.Set();

                return Task.FromResult(true);
            });

            // execute
            topologyOperator.Start(topology.DetermineReceiveResources("sales"), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create("sales", null, AzureServiceBusConnectionString.Value);
            await sender.Send(new BrokeredMessage());

            await Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(received);
            Assert.IsFalse(completeCalled);

            // cleanup 
            await topologyOperator.Stop();

            await Cleanup(container, "sales");
        }

        [Test]
        public async Task Calls_on_error_when_error_during_processing()
        {
            // setting up the environment
            var container = new TransportPartsContainer();

            var topology = await SetupBasicTopology(container, "sales");

            // setup the operator
            var topologyOperator = (IOperateTopology)container.Resolve(typeof(TopologyOperator));

            var completed = new AsyncAutoResetEvent(false);
            var error = new AsyncAutoResetEvent(false);

            var received = false;
            var errorOccured = false;

            topologyOperator.OnIncomingMessage(async (message, context) =>
            {
                received = true;

                var ctx = (BrokeredMessageReceiveContext)context;

                ctx.OnComplete.Add(() =>
                {
                    completed.Set();

                    return Task.FromResult(true);
                });

                await Task.Delay(1).ConfigureAwait(false);
                throw new Exception("Something went wrong");
            });
            topologyOperator.OnError(exception =>
            {
                errorOccured = true;

                error.Set();

                return Task.FromResult(true);
            });

            // execute
            topologyOperator.Start(topology.DetermineReceiveResources("sales"), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create("sales", null, AzureServiceBusConnectionString.Value);
            await sender.Send(new BrokeredMessage());

            await Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(received);
            Assert.IsTrue(errorOccured);

            // cleanup 
            await topologyOperator.Stop();

            await Cleanup(container, "sales");
        }

        [Test]
        public async Task Calls_on_error_when_error_during_completion()
        {
            // setting up the environment
            var container = new TransportPartsContainer();

            var topology = await SetupBasicTopology(container, "sales");

            // setup the operator
            var topologyOperator = (IOperateTopology)container.Resolve(typeof(TopologyOperator));

            var completed = new AsyncAutoResetEvent(false);
            var error = new AsyncAutoResetEvent(false);

            var received = false;
            Exception ex = null;
            var errorOccured = false;

            topologyOperator.OnIncomingMessage((message, context) =>
            {
                received = true;

                var ctx = (BrokeredMessageReceiveContext)context;

                ctx.OnComplete.Add(async () =>
                {
                    await Task.Delay(1).ConfigureAwait(false);
                    throw new Exception("Something went wrong");
                });

                return Task.FromResult(true);
            });
            topologyOperator.OnError(exception =>
            {
                errorOccured = true;

                ex = exception;

                error.Set();

                return Task.FromResult(true);
            });

            // execute
            topologyOperator.Start(topology.DetermineReceiveResources("sales"), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create("sales", null, AzureServiceBusConnectionString.Value);
            await sender.Send(new BrokeredMessage());

            await Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(received);
            Assert.IsTrue(errorOccured);
            Assert.AreEqual("Something went wrong", ex.Message);

            // cleanup 
            await topologyOperator.Stop();

            await Cleanup(container, "sales");
        }

        [Test]
        public async Task Completes_incoming_message_when_successfully_received()
        {
            // setting up the environment
            var container = new TransportPartsContainer();

            var topology = await SetupBasicTopology(container, "sales");

            // setup the operator
            var topologyOperator = (IOperateTopology)container.Resolve(typeof(TopologyOperator));

            var completed = new AsyncAutoResetEvent(false);
            var error = new AsyncAutoResetEvent(false);

            var received = false;
            Exception ex = null;

            topologyOperator.OnIncomingMessage((message, context) =>
            {
                received = true;

                completed.Set();

                return Task.FromResult(true);
            });
            topologyOperator.OnError(exception =>
            {
                ex = exception;

                error.Set();

                return Task.FromResult(true);
            });

            // execute
            topologyOperator.Start(topology.DetermineReceiveResources("sales"), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create("sales", null, AzureServiceBusConnectionString.Value);
            await sender.Send(new BrokeredMessage());

            await Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(received);
            Assert.IsNull(ex);

            await Task.Delay(TimeSpan.FromSeconds(5)); // give asb some time to update stats

            var namespaceLifeCycle = (IManageNamespaceManagerLifeCycle)container.Resolve(typeof(IManageNamespaceManagerLifeCycle));
            var namespaceManager = namespaceLifeCycle.Get(AzureServiceBusConnectionString.Value);
            var queueDescription = await namespaceManager.GetQueue("sales");
            Assert.AreEqual(0, queueDescription.MessageCount);

            // cleanup
            await topologyOperator.Stop();
             
            await Cleanup(container, "sales");
        }

        [Test]
        public async Task Aborts_incoming_message_when_error_during_processing()
        {
            // setting up the environment
            var container = new TransportPartsContainer();

            var topology = await SetupBasicTopology(container, "sales");

            // setup the operator
            var topologyOperator = (IOperateTopology)container.Resolve(typeof(TopologyOperator));

            var completed = new AsyncAutoResetEvent(false);
            var error = new AsyncAutoResetEvent(false);

            var received = false;
            var errorOccured = false;

            topologyOperator.OnIncomingMessage(async (message, context) =>
            {
                received = true;
                await Task.Delay(1).ConfigureAwait(false);
                throw new Exception("Something went wrong");
            });
            topologyOperator.OnError(exception =>
            {
                errorOccured = true;

                error.Set();

                return Task.FromResult(true);
            });

            // execute
            topologyOperator.Start(topology.DetermineReceiveResources("sales"), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create("sales", null, AzureServiceBusConnectionString.Value);
            await sender.Send(new BrokeredMessage());

            await Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(received);
            Assert.IsTrue(errorOccured);

            await Task.Delay(TimeSpan.FromSeconds(5)); // give asb some time to update stats

            var namespaceLifeCycle = (IManageNamespaceManagerLifeCycle)container.Resolve(typeof(IManageNamespaceManagerLifeCycle));
            var namespaceManager = namespaceLifeCycle.Get(AzureServiceBusConnectionString.Value);
            var queueDescription = await namespaceManager.GetQueue("sales");
            Assert.AreEqual(1, queueDescription.MessageCount);

            // cleanup 
            await topologyOperator.Stop();

            await Cleanup(container, "sales");
        }

        [Test]
        public async Task Aborts_incoming_message_when_error_during_completion()
        {
            // setting up the environment
            var container = new TransportPartsContainer();

            var topology = await SetupBasicTopology(container, "sales");

            // setup the operator
            var topologyOperator = (IOperateTopology)container.Resolve(typeof(TopologyOperator));

            var completed = new AsyncAutoResetEvent(false);
            var error = new AsyncAutoResetEvent(false);

            var received = false;
            var errorOccured = false;

            topologyOperator.OnIncomingMessage((message, context) =>
            {
                received = true;

                var ctx = (BrokeredMessageReceiveContext)context;

                ctx.OnComplete.Add(async () =>
                {
                    await Task.Delay(1).ConfigureAwait(false);
                    throw new Exception("Something went wrong");
                });

                return Task.FromResult(true);
            });
            topologyOperator.OnError(exception =>
            {
                errorOccured = true;

                error.Set();

                return Task.FromResult(true);
            });

            // execute
            topologyOperator.Start(topology.DetermineReceiveResources("sales"), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create("sales", null, AzureServiceBusConnectionString.Value);
            await sender.Send(new BrokeredMessage());

            await Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(received);
            Assert.IsTrue(errorOccured);

            await Task.Delay(TimeSpan.FromSeconds(5)); // give asb some time to update stats

            var namespaceLifeCycle = (IManageNamespaceManagerLifeCycle)container.Resolve(typeof(IManageNamespaceManagerLifeCycle));
            var namespaceManager = namespaceLifeCycle.Get(AzureServiceBusConnectionString.Value);
            var queueDescription = await namespaceManager.GetQueue("sales");
            Assert.AreEqual(1, queueDescription.MessageCount);

            // cleanup 
            await topologyOperator.Stop();

            await Cleanup(container, "sales");
        }

        static async Task Cleanup(TransportPartsContainer container, string enpointname)
        {
            var namespaceLifeCycle = (IManageNamespaceManagerLifeCycle) container.Resolve(typeof(IManageNamespaceManagerLifeCycle));
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
            var topologyCreator = (ICreateTopology) container.Resolve(typeof(TopologyCreator));

            var sectionManager = container.Resolve<ITopologySectionManager>();
            await topologyCreator.Create(sectionManager.DetermineResourcesToCreate());
            return sectionManager;
        }
    }
}
