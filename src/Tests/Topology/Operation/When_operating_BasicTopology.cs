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
            var container = new FuncBuilder();

            var topology = await SetupBasicTopology(container, "sales");

            // setup the operator
            var topologyOperator = (IOperateTopology)container.Build(typeof(TopologyOperator));

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
            topologyOperator.Start(topology.DetermineReceiveResources(), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Build(typeof(MessageSenderCreator));
            var sender = await senderFactory.CreateAsync("sales", null, AzureServiceBusConnectionString.Value);
            await sender.SendAsync(new BrokeredMessage());

            await Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(received);
            Assert.IsNull(ex);

            // cleanup 
            await topologyOperator.StopAsync();

            await Cleanup(container, "sales");
        }

        [Test]
        public async Task Calls_completion_callbacks_before_completing()
        {
            // setting up the environment
            var container = new FuncBuilder();

            var topology = await SetupBasicTopology(container, "sales");

            // setup the operator
            var topologyOperator = (IOperateTopology)container.Build(typeof(TopologyOperator));

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
            topologyOperator.Start(topology.DetermineReceiveResources(), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Build(typeof(MessageSenderCreator));
            var sender = await senderFactory.CreateAsync("sales", null, AzureServiceBusConnectionString.Value);
            await sender.SendAsync(new BrokeredMessage());

            await Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(completeCalled);
            Assert.IsTrue(received);
            Assert.IsNull(ex);

            // cleanup 
            await topologyOperator.StopAsync();

            await Cleanup(container, "sales");
        }

        [Test]
        public async Task Does_not_call_completion_when_error_during_processing()
        {
            // setting up the environment
            var container = new FuncBuilder();

            var topology = await SetupBasicTopology(container, "sales");

            // setup the operator
            var topologyOperator = (IOperateTopology)container.Build(typeof(TopologyOperator));

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
            topologyOperator.Start(topology.DetermineReceiveResources(), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Build(typeof(MessageSenderCreator));
            var sender = await senderFactory.CreateAsync("sales", null, AzureServiceBusConnectionString.Value);
            await sender.SendAsync(new BrokeredMessage());

            await Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(received);
            Assert.IsFalse(completeCalled);

            // cleanup 
            await topologyOperator.StopAsync();

            await Cleanup(container, "sales");
        }

        [Test]
        public async Task Calls_on_error_when_error_during_processing()
        {
            // setting up the environment
            var container = new FuncBuilder();

            var topology = await SetupBasicTopology(container, "sales");

            // setup the operator
            var topologyOperator = (IOperateTopology)container.Build(typeof(TopologyOperator));

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
            topologyOperator.Start(topology.DetermineReceiveResources(), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Build(typeof(MessageSenderCreator));
            var sender = await senderFactory.CreateAsync("sales", null, AzureServiceBusConnectionString.Value);
            await sender.SendAsync(new BrokeredMessage());

            await Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(received);
            Assert.IsTrue(errorOccured);

            // cleanup 
            await topologyOperator.StopAsync();

            await Cleanup(container, "sales");
        }

        [Test]
        public async Task Calls_on_error_when_error_during_completion()
        {
            // setting up the environment
            var container = new FuncBuilder();

            var topology = await SetupBasicTopology(container, "sales");

            // setup the operator
            var topologyOperator = (IOperateTopology)container.Build(typeof(TopologyOperator));

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
            topologyOperator.Start(topology.DetermineReceiveResources(), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Build(typeof(MessageSenderCreator));
            var sender = await senderFactory.CreateAsync("sales", null, AzureServiceBusConnectionString.Value);
            await sender.SendAsync(new BrokeredMessage());

            await Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(received);
            Assert.IsTrue(errorOccured);
            Assert.AreEqual("Something went wrong", ex.Message);

            // cleanup 
            await topologyOperator.StopAsync();

            await Cleanup(container, "sales");
        }

        [Test]
        public async Task Completes_incoming_message_when_successfully_received()
        {
            // setting up the environment
            var container = new FuncBuilder();

            var topology = await SetupBasicTopology(container, "sales");

            // setup the operator
            var topologyOperator = (IOperateTopology)container.Build(typeof(TopologyOperator));

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
            topologyOperator.Start(topology.DetermineReceiveResources(), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Build(typeof(MessageSenderCreator));
            var sender = await senderFactory.CreateAsync("sales", null, AzureServiceBusConnectionString.Value);
            await sender.SendAsync(new BrokeredMessage());

            await Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(received);
            Assert.IsNull(ex);

            await Task.Delay(TimeSpan.FromSeconds(5)); // give asb some time to update stats

            var namespaceLifeCycle = (IManageNamespaceManagerLifeCycle)container.Build(typeof(IManageNamespaceManagerLifeCycle));
            var namespaceManager = namespaceLifeCycle.Get(AzureServiceBusConnectionString.Value);
            var queueDescription = await namespaceManager.GetQueueAsync("sales");
            Assert.AreEqual(0, queueDescription.MessageCount);

            // cleanup
            await topologyOperator.StopAsync();
             
            await Cleanup(container, "sales");
        }

        [Test]
        public async Task Aborts_incoming_message_when_error_during_processing()
        {
            // setting up the environment
            var container = new FuncBuilder();

            var topology = await SetupBasicTopology(container, "sales");

            // setup the operator
            var topologyOperator = (IOperateTopology)container.Build(typeof(TopologyOperator));

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
            topologyOperator.Start(topology.DetermineReceiveResources(), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Build(typeof(MessageSenderCreator));
            var sender = await senderFactory.CreateAsync("sales", null, AzureServiceBusConnectionString.Value);
            await sender.SendAsync(new BrokeredMessage());

            await Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(received);
            Assert.IsTrue(errorOccured);

            await Task.Delay(TimeSpan.FromSeconds(5)); // give asb some time to update stats

            var namespaceLifeCycle = (IManageNamespaceManagerLifeCycle)container.Build(typeof(IManageNamespaceManagerLifeCycle));
            var namespaceManager = namespaceLifeCycle.Get(AzureServiceBusConnectionString.Value);
            var queueDescription = await namespaceManager.GetQueueAsync("sales");
            Assert.AreEqual(1, queueDescription.MessageCount);

            // cleanup 
            await topologyOperator.StopAsync();

            await Cleanup(container, "sales");
        }

        [Test]
        public async Task Aborts_incoming_message_when_error_during_completion()
        {
            // setting up the environment
            var container = new FuncBuilder();

            var topology = await SetupBasicTopology(container, "sales");

            // setup the operator
            var topologyOperator = (IOperateTopology)container.Build(typeof(TopologyOperator));

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
            topologyOperator.Start(topology.DetermineReceiveResources(), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Build(typeof(MessageSenderCreator));
            var sender = await senderFactory.CreateAsync("sales", null, AzureServiceBusConnectionString.Value);
            await sender.SendAsync(new BrokeredMessage());

            await Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(received);
            Assert.IsTrue(errorOccured);

            await Task.Delay(TimeSpan.FromSeconds(5)); // give asb some time to update stats

            var namespaceLifeCycle = (IManageNamespaceManagerLifeCycle)container.Build(typeof(IManageNamespaceManagerLifeCycle));
            var namespaceManager = namespaceLifeCycle.Get(AzureServiceBusConnectionString.Value);
            var queueDescription = await namespaceManager.GetQueueAsync("sales");
            Assert.AreEqual(1, queueDescription.MessageCount);

            // cleanup 
            await topologyOperator.StopAsync();

            await Cleanup(container, "sales");
        }

        static async Task Cleanup(FuncBuilder container, string enpointname)
        {
            var namespaceLifeCycle = (IManageNamespaceManagerLifeCycle) container.Build(typeof(IManageNamespaceManagerLifeCycle));
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
            var topologyCreator = (ICreateTopology) container.Build(typeof(TopologyCreator));
            await topologyCreator.Create(topology.DetermineResourcesToCreate());
            return topology;
        }
    }
}
