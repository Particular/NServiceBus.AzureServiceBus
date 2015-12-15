namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Seam
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.AzureServiceBus.Tests;
    using NServiceBus.DeliveryConstraints;
    using NServiceBus.Routing;
    using NServiceBus.Settings;
    using NServiceBus.Transports;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_dispatching_messages_in_receive_context
    {
        [Test]
        public async Task Should_dispatch_message_in_receive_context()
        {
            var completed = new AsyncAutoResetEvent(false);

            // setting up the environment
            var container = new TransportPartsContainer();
            var settings = new SettingsHolder();

            // setup a basic topologySectionManager for testing
            var topology = await SetupBasicTopology(container, "sales", settings);

            // setup the receive side of things
            var topologyOperator = (IOperateTopology)container.Resolve(typeof(TopologyOperator));
            var pump = new MessagePump(topology, topologyOperator);
            
            // setup the dispatching side of things
            var dispatcher = (IDispatchMessages)container.Resolve(typeof(IDispatchMessages));

            // create the destination queue
            var namespaceLifeCycle = (IManageNamespaceManagerLifeCycle)container.Resolve(typeof(IManageNamespaceManagerLifeCycle));
            var creator = (ICreateAzureServiceBusQueues)container.Resolve(typeof(ICreateAzureServiceBusQueues));
            var namespaceManager = namespaceLifeCycle.Get(AzureServiceBusConnectionString.Value);
            await creator.Create("myqueue", namespaceManager);

            // setup the test
            var received = false;

            // Dummy CriticalError
            var criticalError = new CriticalError((endpoint, error, exception) => Task.FromResult(0));

            await pump.Init(async context =>
            {
                received = true;

                var bytes = Encoding.UTF8.GetBytes("Whatever");
                var outgoingMessage = new OutgoingMessage("Id-1", new Dictionary<string, string>(), bytes);
                var dispatchOptions = new DispatchOptions(new UnicastAddressTag("myqueue"), DispatchConsistency.Default, Enumerable.Empty<DeliveryConstraint>());

                var ctx = context.Context.Get<ReceiveContext>();
                ctx.OnComplete.Add(() =>
                {
                    completed.Set();
                    return TaskEx.Completed;
                });

                await dispatcher.Dispatch(new[]
                {
                    new TransportOperation(outgoingMessage, dispatchOptions)
                }, context.Context); // makes sure the context propagates

                // TODO: TransportTransactionMode will need to change with topology
            }, criticalError, new PushSettings("sales", "error", false, TransportTransactionMode.SendsAtomicWithReceive));

            // start the pump
            pump.Start(new PushRuntimeSettings(1));

            // send message to local queue
            var senderFactory = (MessageSenderCreator)container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create("sales", null, AzureServiceBusConnectionString.Value);
            await sender.Send(new BrokeredMessage());

            await completed.WaitOne();

            await Task.Delay(TimeSpan.FromSeconds(3)); //the OnCompleted callbacks are called right before the batch is completed, so give it a second to do that

            // validate
            Assert.IsTrue(received);

            // check destination queue for dispatched message
            var queue = await namespaceManager.GetQueue("myqueue");
            Assert.IsTrue(queue.MessageCount == 1, "'myqueue' was expected to have 1 message, but it didn't");

            // cleanup 
            await pump.Stop();

            await Cleanup(container, "sales", "myqueue");
        }

        [Test]
        public async Task Will_not_rollback_dispatch_message_in_receive_context_when_exception_occurs_on_completion()
        {
            var errored = new AsyncAutoResetEvent(false);

            // setting up the environment
            var container = new TransportPartsContainer();
            var settings = new SettingsHolder();

            // setup a basic topologySectionManager for testing
            var topology = await SetupBasicTopology(container, "sales", settings);

            // setup the receive side of things
            var topologyOperator = (IOperateTopology)container.Resolve(typeof(TopologyOperator));
            var pump = new MessagePump(topology, topologyOperator);

            // setup the dispatching side of things
            var dispatcher = (IDispatchMessages)container.Resolve(typeof(IDispatchMessages));

            // create the destination queue
            var namespaceLifeCycle = (IManageNamespaceManagerLifeCycle)container.Resolve(typeof(IManageNamespaceManagerLifeCycle));
            var creator = (ICreateAzureServiceBusQueues)container.Resolve(typeof(ICreateAzureServiceBusQueues));
            var namespaceManager = namespaceLifeCycle.Get(AzureServiceBusConnectionString.Value);
            await creator.Create("myqueue", namespaceManager);

            // setup the test
            var received = false;

            pump.OnError(exception =>
            {
                errored.Set();

                return TaskEx.Completed;
            });

            // Dummy CriticalError
            var criticalError = new CriticalError((endpoint, error, exception) => Task.FromResult(0));

            await pump.Init(async context =>
            {
                received = true;

                var bytes = Encoding.UTF8.GetBytes("Whatever");
                var outgoingMessage = new OutgoingMessage("Id-1", new Dictionary<string, string>(), bytes);
                var dispatchOptions = new DispatchOptions(new UnicastAddressTag("myqueue"), DispatchConsistency.Default, Enumerable.Empty<DeliveryConstraint>());

                var ctx = context.Context.Get<ReceiveContext>();
                ctx.OnComplete.Add(async () =>
                {
                    await Task.Delay(1); // makes sure the compiler creates the statemachine that handles exception propagation

                    throw new Exception("Something bad happens on complete");
                });

                await dispatcher.Dispatch(new[]
                {
                    new TransportOperation(outgoingMessage, dispatchOptions)
                }, context.Context);

                // TODO: TransportTransactionMode will need to change with topology
            }, criticalError, new PushSettings("sales", "error", false, TransportTransactionMode.SendsAtomicWithReceive));

            // start the pump
            pump.Start(new PushRuntimeSettings(1));

            // send message to local queue
            var senderFactory = (MessageSenderCreator)container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create("sales", null, AzureServiceBusConnectionString.Value);
            await sender.Send(new BrokeredMessage());

            await errored.WaitOne();

            await Task.Delay(TimeSpan.FromSeconds(3)); //the OnCompleted callbacks are called right before the batch is completed, so give it a second to do that

            // stop the pump so retries don't keep going
            await pump.Stop();

            // validate
            Assert.IsTrue(received);

            // check destination queue that message has indeed been dispatched
            var queue = await namespaceManager.GetQueue("myqueue");
            Assert.GreaterOrEqual(queue.MessageCount, 1, "'myqueue' was expected to have 1 or more messages, but it didn't");

            // check origin queue that source message is still there
            queue = await namespaceManager.GetQueue("sales");
            Assert.AreEqual(1, queue.MessageCount, "'sales' was expected to have 1 message, but it didn't");

            // cleanup
            await Cleanup(container, "sales", "myqueue");
        }


        [Test]
        public async Task Should_dispatch_message_in_receive_context_via_receive_queue()
        {
            var completed = new AsyncAutoResetEvent(false);

            // setting up the environment
            var container = new TransportPartsContainer();
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.Connectivity().SendViaReceiveQueue(true);

            // setup a basic topologySectionManager for testing
            var topology = await SetupBasicTopology(container, "sales", settings);

            // setup the receive side of things
            var topologyOperator = (IOperateTopology)container.Resolve(typeof(TopologyOperator));
            var pump = new MessagePump(topology, topologyOperator);

            // setup the dispatching side of things
            var dispatcher = (IDispatchMessages)container.Resolve(typeof(IDispatchMessages));

            // create the destination queue
            var namespaceLifeCycle = (IManageNamespaceManagerLifeCycle)container.Resolve(typeof(IManageNamespaceManagerLifeCycle));
            var creator = (ICreateAzureServiceBusQueues)container.Resolve(typeof(ICreateAzureServiceBusQueues));
            var namespaceManager = namespaceLifeCycle.Get(AzureServiceBusConnectionString.Value);
            await creator.Create("myqueue", namespaceManager);

            // setup the test
            var received = false;

            // Dummy CriticalError
            var criticalError = new CriticalError((endpoint, error, exception) => Task.FromResult(0));

            await pump.Init(async context =>
            {
                received = true;

                var bytes = Encoding.UTF8.GetBytes("Whatever");
                var outgoingMessage = new OutgoingMessage("Id-1", new Dictionary<string, string>(), bytes);
                var dispatchOptions = new DispatchOptions(new UnicastAddressTag("myqueue"), DispatchConsistency.Default, Enumerable.Empty<DeliveryConstraint>());

                var ctx = context.Context.Get<ReceiveContext>();
                ctx.OnComplete.Add(() =>
                {
                    completed.Set();
                    return TaskEx.Completed;
                });

                await dispatcher.Dispatch(new[]
                {
                    new TransportOperation(outgoingMessage, dispatchOptions)
                }, context.Context); // makes sure the context propagates

                // TODO: TransportTransactionMode will need to change with topology
            }, criticalError, new PushSettings("sales", "error", false, TransportTransactionMode.SendsAtomicWithReceive));

            // start the pump
            pump.Start(new PushRuntimeSettings(1));

            // send message to local queue
            var senderFactory = (MessageSenderCreator)container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create("sales", null, AzureServiceBusConnectionString.Value);
            await sender.Send(new BrokeredMessage {MessageId = "id-init"});

            await completed.WaitOne();

            await Task.Delay(TimeSpan.FromSeconds(5)); //the OnCompleted callbacks are called right before the batch is completed, so give it a second to do that

            // validate
            Assert.IsTrue(received);

            // check destination queue for dispatched message
            var queue = await namespaceManager.GetQueue("myqueue");
            var count = queue.MessageCount;
            Assert.IsTrue(count == 1, "'myqueue' was expected to have 1 message, but it had " + count + " instead");

            // cleanup 
            await pump.Stop();

            await Cleanup(container, "sales", "myqueue");
        }

        [Test]
        public async Task Should_rollback_dispatch_message_in_receive_context_via_receive_queue_when_exception_occurs_on_completion()
        {
            var errored = new AsyncAutoResetEvent(false);

            // setting up the environment
            var container = new TransportPartsContainer();
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.Connectivity().SendViaReceiveQueue(true);

            // setup a basic topologySectionManager for testing
            var topology = await SetupBasicTopology(container, "sales", settings);

            // setup the receive side of things
            var topologyOperator = (IOperateTopology)container.Resolve(typeof(TopologyOperator));
            var pump = new MessagePump(topology, topologyOperator);

            // setup the dispatching side of things
            var dispatcher = (IDispatchMessages)container.Resolve(typeof(IDispatchMessages));

            // create the destination queue
            var namespaceLifeCycle = (IManageNamespaceManagerLifeCycle)container.Resolve(typeof(IManageNamespaceManagerLifeCycle));
            var creator = (ICreateAzureServiceBusQueues)container.Resolve(typeof(ICreateAzureServiceBusQueues));
            var namespaceManager = namespaceLifeCycle.Get(AzureServiceBusConnectionString.Value);
            await creator.Create("myqueue", namespaceManager);

            // setup the test
            var received = false;

            pump.OnError(exception =>
            {
                errored.Set();

                return TaskEx.Completed;
            });

            // Dummy CriticalError
            var criticalError = new CriticalError((endpoint, error, exception) => Task.FromResult(0));

            await pump.Init(async context =>
            {
                received = true;

                var bytes = Encoding.UTF8.GetBytes("Whatever");
                var outgoingMessage = new OutgoingMessage("Id-1", new Dictionary<string, string>(), bytes);
                var dispatchOptions = new DispatchOptions(new UnicastAddressTag("myqueue"), DispatchConsistency.Default, Enumerable.Empty<DeliveryConstraint>());

                var ctx = context.Context.Get<ReceiveContext>();
                ctx.OnComplete.Add(async () =>
                {
                    await Task.Delay(1); // makes sure the compiler creates the statemachine that handles exception propagation
                    
                    throw new Exception("Something bad happens on complete");
                });

                await dispatcher.Dispatch(new[]
                {
                    new TransportOperation(outgoingMessage, dispatchOptions)
                }, context.Context);

                // TODO: TransportTransactionMode will need to change with topology
            }, criticalError, new PushSettings("sales", "error", false, TransportTransactionMode.SendsAtomicWithReceive));

            // start the pump
            pump.Start(new PushRuntimeSettings(1));

            // send message to local queue
            var senderFactory = (MessageSenderCreator)container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create("sales", null, AzureServiceBusConnectionString.Value);
            await sender.Send(new BrokeredMessage());

            await errored.WaitOne();

            await Task.Delay(TimeSpan.FromSeconds(3)); //the OnCompleted callbacks are called right before the batch is completed, so give it a second to do that

            // stop the pump so retries don't keep going
            await pump.Stop();

            // validate
            Assert.IsTrue(received);

            // check destination queue that message has not been dispatched
            var queue = await namespaceManager.GetQueue("myqueue");
            Assert.AreEqual(0, queue.MessageCount, $"'myqueue' was expected to have no messages, but it did ({queue.MessageCount})");

            // check origin queue that source message is still there
            queue = await namespaceManager.GetQueue("sales");
            Assert.AreEqual(1, queue.MessageCount, "'sales' was expected to have 1 message, but it didn't");

            // cleanup
            await Cleanup(container, "sales", "myqueue");
        }

        [Test]
        public async Task Should_retry_after_rollback_in_less_then_thirty_seconds_when_using_via_queue()
        {
            var retried = new AsyncAutoResetEvent(false);
            var invocationCount = 0;
            var firstTime = DateTime.MinValue;
            var secondTime = DateTime.MaxValue;

            // setting up the environment
            var container = new TransportPartsContainer();
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.Connectivity().SendViaReceiveQueue(true);

            // setup a basic topologySectionManager for testing
            var topology = await SetupBasicTopology(container, "sales", settings);

            // setup the receive side of things
            var topologyOperator = (IOperateTopology)container.Resolve(typeof(TopologyOperator));
            var pump = new MessagePump(topology, topologyOperator);

            // setup the dispatching side of things
            var dispatcher = (IDispatchMessages)container.Resolve(typeof(IDispatchMessages));

            // create the destination queue
            var namespaceLifeCycle = (IManageNamespaceManagerLifeCycle)container.Resolve(typeof(IManageNamespaceManagerLifeCycle));
            var creator = (ICreateAzureServiceBusQueues)container.Resolve(typeof(ICreateAzureServiceBusQueues));
            var namespaceManager = namespaceLifeCycle.Get(AzureServiceBusConnectionString.Value);
            await creator.Create("myqueue", namespaceManager);

            // setup the test
            pump.OnError(exception =>
            {
                invocationCount++;

                if (invocationCount == 1)
                {
                    firstTime = DateTime.Now;
                }

                if (invocationCount == 2)
                {
                    secondTime = DateTime.Now;
                    retried.Set();
                }

                return TaskEx.Completed;
            });

            // Dummy CriticalError
            var criticalError = new CriticalError((endpoint, error, exception) => Task.FromResult(0));

            await pump.Init(async context =>
            {
                var bytes = Encoding.UTF8.GetBytes("Whatever");
                var outgoingMessage = new OutgoingMessage("Id-1", new Dictionary<string, string>(), bytes);
                var dispatchOptions = new DispatchOptions(new UnicastAddressTag("myqueue"), DispatchConsistency.Default, Enumerable.Empty<DeliveryConstraint>());

                var ctx = context.Context.Get<ReceiveContext>();
                ctx.OnComplete.Add(async () =>
                {
                    await Task.Delay(1); // makes sure the compiler creates the statemachine that handles exception propagation

                    throw new Exception("Something bad happens on complete");
                });

                await dispatcher.Dispatch(new[]
                {
                    new TransportOperation(outgoingMessage, dispatchOptions)
                }, context.Context);

                // TODO: TransportTransactionMode will need to change with topology
            }, criticalError, new PushSettings("sales", "error", false, TransportTransactionMode.SendsAtomicWithReceive));

            // start the pump
            pump.Start(new PushRuntimeSettings(1));

            // send message to local queue
            var senderFactory = (MessageSenderCreator)container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create("sales", null, AzureServiceBusConnectionString.Value);
            await sender.Send(new BrokeredMessage());

            await retried.WaitOne();

            await Task.Delay(TimeSpan.FromSeconds(3)); //the OnCompleted callbacks are called right before the batch is completed, so give it a second to do that

            // stop the pump so retries don't keep going
            await pump.Stop();

            // validate
            var elapsed = secondTime - firstTime;
            Console.WriteLine("elapsed" + elapsed.TotalSeconds);
            Assert.IsTrue(elapsed < TimeSpan.FromSeconds(29)); // 29 instead of 30 to accommodate for a little clock drift

            // cleanup
            await Cleanup(container, "sales", "myqueue");
        }

        static async Task Cleanup(TransportPartsContainer container, params string[] endpointnames)
        {
            var namespaceLifeCycle = (IManageNamespaceManagerLifeCycle)container.Resolve(typeof(IManageNamespaceManagerLifeCycle));
            var namespaceManager = namespaceLifeCycle.Get(AzureServiceBusConnectionString.Value);

            foreach (var endpointname in endpointnames)
            {
                await namespaceManager.DeleteQueue(endpointname);
            }
        }

        async Task<ITopologySectionManager> SetupBasicTopology(TransportPartsContainer container, string enpointname, SettingsHolder settings)
        {
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