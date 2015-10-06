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
            await topologyOperator.Start(topology.Determine(Purpose.Receiving), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Build(typeof(MessageSenderCreator));
            var sender = (IMessageSender) await senderFactory.CreateAsync("sales", AzureServiceBusConnectionString.Value);
            await sender.SendAsync(new BrokeredMessage());

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
            await topologyOperator.Start(topology.Determine(Purpose.Receiving), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Build(typeof(MessageSenderCreator));
            var sender = (IMessageSender)await senderFactory.CreateAsync("sales", AzureServiceBusConnectionString.Value);
            await sender.SendAsync(new BrokeredMessage());

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
            var container = new FuncBuilder();

            var topology = await SetupBasicTopology(container, "sales");

            // setup the operator
            var topologyOperator = (IOperateTopology)container.Build(typeof(TopologyOperator));

            var completed = new AsyncAutoResetEvent(false);
            var error = new AsyncAutoResetEvent(false);

            var received = false;
            var completeCalled = false;

            topologyOperator.OnIncomingMessage((message, context) =>
            {
                received = true;

                var ctx = (BrokeredMessageReceiveContext)context;

                ctx.OnComplete.Add(() =>
                {
                    completeCalled = true;

                    completed.Set();

                    return Task.FromResult(true);
                });

                throw new Exception("Something went wrong");
            });
            topologyOperator.OnError(exception =>
            {
                error.Set();

                return Task.FromResult(true);
            });

            // execute
            await topologyOperator.Start(topology.Determine(Purpose.Receiving), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Build(typeof(MessageSenderCreator));
            var sender = (IMessageSender)await senderFactory.CreateAsync("sales", AzureServiceBusConnectionString.Value);
            await sender.SendAsync(new BrokeredMessage());

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

                ctx.OnComplete.Add(() =>
                {
                    completed.Set();

                    return Task.FromResult(true);
                });

                throw new Exception("Something went wrong");
            });
            topologyOperator.OnError(exception =>
            {
                errorOccured = true;

                error.Set();

                return Task.FromResult(true);
            });

            // execute
            await topologyOperator.Start(topology.Determine(Purpose.Receiving), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Build(typeof(MessageSenderCreator));
            var sender = (IMessageSender)await senderFactory.CreateAsync("sales", AzureServiceBusConnectionString.Value);
            await sender.SendAsync(new BrokeredMessage());

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

                ctx.OnComplete.Add(() =>
                {
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
            await topologyOperator.Start(topology.Determine(Purpose.Receiving), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Build(typeof(MessageSenderCreator));
            var sender = (IMessageSender)await senderFactory.CreateAsync("sales", AzureServiceBusConnectionString.Value);
            await sender.SendAsync(new BrokeredMessage());

            await Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(received);
            Assert.IsTrue(errorOccured);
            Assert.AreEqual("One or more errors occurred.", ex.Message);
            Assert.AreEqual("Something went wrong", ex.InnerException.Message);

            // cleanup 
            await topologyOperator.Stop();

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
            await topologyOperator.Start(topology.Determine(Purpose.Receiving), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Build(typeof(MessageSenderCreator));
            var sender = (IMessageSender)await senderFactory.CreateAsync("sales", AzureServiceBusConnectionString.Value);
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
            await topologyOperator.Stop();
             
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

            topologyOperator.OnIncomingMessage((message, context) =>
            {
                received = true;

                throw new Exception("Something went wrong");
            });
            topologyOperator.OnError(exception =>
            {
                errorOccured = true;

                error.Set();

                return Task.FromResult(true);
            });

            // execute
            await topologyOperator.Start(topology.Determine(Purpose.Receiving), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Build(typeof(MessageSenderCreator));
            var sender = (IMessageSender)await senderFactory.CreateAsync("sales", AzureServiceBusConnectionString.Value);
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
            await topologyOperator.Stop();

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

                ctx.OnComplete.Add(() =>
                {
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
            await topologyOperator.Start(topology.Determine(Purpose.Receiving), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Build(typeof(MessageSenderCreator));
            var sender = (IMessageSender)await senderFactory.CreateAsync("sales", AzureServiceBusConnectionString.Value);
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
            await topologyOperator.Stop();

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
            await topologyCreator.Create(topology.Determine(Purpose.Creating));
            return topology;
        }
    }
}
