namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology.Operation
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Azure.WindowsAzureServiceBus.Tests;
    using NServiceBus.Azure.WindowsAzureServiceBus.Tests.Receiving;
    using NServiceBus.Azure.WindowsAzureServiceBus.Tests.TestUtils;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.Routing;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_operating_StandardTopology
    {
        [Test]
        public async Task Receives_incoming_messages_from_endpoint_queue()
        {
            // cleanup
            await TestUtility.Delete("sales");

            // setting up the environment
            var container = new TransportPartsContainer();

            var topology = await SetupStandardTopology(container, "sales");

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
            var senderFactory = (MessageSenderCreator)container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create("sales", null, "namespace");
            await sender.Send(new BrokeredMessage());

            await Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(received);
            Assert.IsNull(ex);
            
            await topologyOperator.Stop();
        }

        [Test]
        public async Task Calls_completion_callbacks_before_completing()
        {
            // cleanup
            await TestUtility.Delete("sales");

            // setting up the environment
            var container = new TransportPartsContainer();

            var topology = await SetupStandardTopology(container, "sales");

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

                    return TaskEx.Completed;
                });

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
            var senderFactory = (MessageSenderCreator)container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create("sales", null, "namespace");
            await sender.Send(new BrokeredMessage());

            await Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(completeCalled);
            Assert.IsTrue(received);
            Assert.IsNull(ex);
            
            await topologyOperator.Stop();
        }

        [Test]
        public async Task Does_not_call_completion_when_error_during_processing()
        {
            // cleanup
            await TestUtility.Delete("sales");

            // setting up the environment
            var container = new TransportPartsContainer();

            var topology = await SetupStandardTopology(container, "sales");

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

                    return TaskEx.Completed;
                });

                await Task.Delay(1).ConfigureAwait(false);
                throw new Exception("Something went wrong");
            });
            topologyOperator.OnError(exception =>
            {
                error.Set();

                return TaskEx.Completed;
            });

            // execute
            topologyOperator.Start(topology.DetermineReceiveResources("sales"), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create("sales", null, "namespace");
            await sender.Send(new BrokeredMessage());

            await Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(received);
            Assert.IsFalse(completeCalled);
            
            await topologyOperator.Stop();
        }

        [Test]
        public async Task Calls_on_error_when_error_during_processing()
        {
            // cleanup
            await TestUtility.Delete("sales");

            // setting up the environment
            var container = new TransportPartsContainer();

            var topology = await SetupStandardTopology(container, "sales");

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

                    return TaskEx.Completed;
                });

                await Task.Delay(1).ConfigureAwait(false);
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
            var senderFactory = (MessageSenderCreator)container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create("sales", null, "namespace");
            await sender.Send(new BrokeredMessage());

            await Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(received);
            Assert.IsTrue(errorOccured);

            await topologyOperator.Stop();
        }

        [Test]
        public async Task Calls_on_error_when_error_during_completion()
        {
            // cleanup
            await TestUtility.Delete("sales");

            // setting up the environment
            var container = new TransportPartsContainer();

            var topology = await SetupStandardTopology(container, "sales");

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

                return TaskEx.Completed;
            });
            topologyOperator.OnError(exception =>
            {
                errorOccured = true;

                ex = exception;

                error.Set();

                return TaskEx.Completed;
            });

            // execute
            topologyOperator.Start(topology.DetermineReceiveResources("sales"), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create("sales", null, "namespace");
            await sender.Send(new BrokeredMessage());

            await Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(received);
            Assert.IsTrue(errorOccured);
            Assert.AreEqual("Something went wrong", ex.Message);

            await topologyOperator.Stop();
        }

        [Test]
        public async Task Completes_incoming_message_when_successfully_received()
        {
            // cleanup
            await TestUtility.Delete("sales");

            // setting up the environment
            var container = new TransportPartsContainer();

            var topology = await SetupStandardTopology(container, "sales");

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
            var senderFactory = (MessageSenderCreator)container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create("sales", null, "namespace");
            await sender.Send(new BrokeredMessage());

            await Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(received);
            Assert.IsNull(ex);

            await Task.Delay(TimeSpan.FromSeconds(5)); // give asb some time to update stats

            var namespaceLifeCycle = (IManageNamespaceManagerLifeCycle)container.Resolve(typeof(IManageNamespaceManagerLifeCycle));
            var namespaceManager = namespaceLifeCycle.Get("namespace");
            var queueDescription = await namespaceManager.GetQueue("sales");
            Assert.AreEqual(0, queueDescription.MessageCount);
            
            await topologyOperator.Stop();
        }

        [Test]
        public async Task Aborts_incoming_message_when_error_during_processing()
        {
            // cleanup
            await TestUtility.Delete("sales");

            // setting up the environment
            var container = new TransportPartsContainer();

            var topology = await SetupStandardTopology(container, "sales");

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

                return TaskEx.Completed;
            });

            // execute
            topologyOperator.Start(topology.DetermineReceiveResources("sales"), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create("sales", null, "namespace");
            await sender.Send(new BrokeredMessage());

            await Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(received);
            Assert.IsTrue(errorOccured);

            await Task.Delay(TimeSpan.FromSeconds(5)); // give asb some time to update stats

            var namespaceLifeCycle = (IManageNamespaceManagerLifeCycle)container.Resolve(typeof(IManageNamespaceManagerLifeCycle));
            var namespaceManager = namespaceLifeCycle.Get("namespace");
            var queueDescription = await namespaceManager.GetQueue("sales");
            Assert.AreEqual(1, queueDescription.MessageCount);

            await topologyOperator.Stop();
        }

        [Test]
        public async Task Aborts_incoming_message_when_error_during_completion()
        {
            // cleanup
            await TestUtility.Delete("sales");

            // setting up the environment
            var container = new TransportPartsContainer();

            var topology = await SetupStandardTopology(container, "sales");

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

                return TaskEx.Completed;
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
            var senderFactory = (MessageSenderCreator)container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create("sales", null, "namespace");
            await sender.Send(new BrokeredMessage());

            await Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(received);
            Assert.IsTrue(errorOccured);

            await Task.Delay(TimeSpan.FromSeconds(5)); // give asb some time to update stats

            var namespaceLifeCycle = (IManageNamespaceManagerLifeCycle)container.Resolve(typeof(IManageNamespaceManagerLifeCycle));
            var namespaceManager = namespaceLifeCycle.Get("namespace");
            var queueDescription = await namespaceManager.GetQueue("sales");
            Assert.AreEqual(1, queueDescription.MessageCount);

            await topologyOperator.Stop();
        }

        async Task<ITopologySectionManager> SetupStandardTopology(TransportPartsContainer container, string enpointname)
        {
            var settings = new SettingsHolder();
            container.Register(typeof(SettingsHolder), () => settings);
            var extensions = new AzureServiceBusTopologySettings(settings);
            settings.SetDefault<EndpointName>(new EndpointName(enpointname));
            extensions.Addressing().NamespacePartitioning().AddNamespace("namespace", AzureServiceBusConnectionString.Value);

            var topology = new StandardTopology(container);
            topology.Initialize(settings);

            // create the topologySectionManager
            var topologyCreator = (ICreateTopology) container.Resolve(typeof(TopologyCreator));

            var sectionManager = container.Resolve<ITopologySectionManager>();
            await topologyCreator.Create(sectionManager.DetermineResourcesToCreate());
            return sectionManager;
        }
    }
}
