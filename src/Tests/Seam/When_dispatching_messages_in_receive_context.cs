namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Seam
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AzureServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Tests;
    using Receiving;
    using TestUtils;
    using Transport.AzureServiceBus;
    using DeliveryConstraints;
    using Transport;
    using Routing;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_dispatching_messages_in_receive_context
    {
        [Test]
        public async Task Should_dispatch_message_in_receive_context()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            // cleanup
            await TestUtility.Delete("sales", "myqueue");

            var completed = new AsyncAutoResetEvent(false);

            // setting up the environment
            var container = new TransportPartsContainer();
            var settings = new SettingsHolder();

            // setup a basic topologySectionManager for testing
            var topology = await SetupEndpointOrientedTopology(container, "sales", settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.SendViaReceiveQueue(true);

            // setup the receive side of things
            var topologyOperator = (IOperateTopology)container.Resolve(typeof(TopologyOperator));
            var pump = new MessagePump(topology, topologyOperator);

            // setup the dispatching side of things
            var dispatcher = (IDispatchMessages)container.Resolve(typeof(IDispatchMessages));

            // create the destination queue
            var namespaceLifeCycle = (IManageNamespaceManagerLifeCycle)container.Resolve(typeof(IManageNamespaceManagerLifeCycle));
            var creator = (ICreateAzureServiceBusQueues)container.Resolve(typeof(ICreateAzureServiceBusQueues));
            var namespaceManager = namespaceLifeCycle.Get("namespaceName");
            await creator.Create("myqueue", namespaceManager);

            // setup the test
            var received = false;

            // Dummy CriticalError
            var criticalError = new CriticalError(ctx => TaskEx.Completed);

            await pump.Init(async context =>
            {
                // normally the core would do that
                context.Context.Set(context.TransportTransaction);

                received = true;

                var bytes = Encoding.UTF8.GetBytes("Whatever");
                var outgoingMessage = new OutgoingMessage("Id-1", new Dictionary<string, string>(), bytes);

                var transportOperations = new TransportOperations(new TransportOperation(outgoingMessage, new UnicastAddressTag("myqueue"), DispatchConsistency.Default, Enumerable.Empty<DeliveryConstraint>().ToList()));

                await dispatcher.Dispatch(transportOperations, new TransportTransaction(), context.Context); // makes sure the context propagates

                completed.Set();
            }, null, criticalError, new PushSettings("sales", "error", false, TransportTransactionMode.ReceiveOnly));

            // start the pump
            pump.Start(new PushRuntimeSettings(1));

            // send message to local queue
            var senderFactory = (MessageSenderCreator)container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create("sales", null, "namespaceName");
            await sender.Send(new BrokeredMessage { MessageId = "id-incoming" });
            
            await completed.WaitAsync(cts.Token);

            await Task.Delay(TimeSpan.FromSeconds(3)); // allow message count to update on queue

            // validate
            Assert.IsTrue(received);

            // check destination queue for dispatched message
            var queue = await namespaceManager.GetQueue("myqueue");
            Assert.IsTrue(queue.MessageCount == 1, "'myqueue' was expected to have 1 message, but it didn't");

            // cleanup
            await pump.Stop();
        }

        [Test]
        public async Task Should_dispatch_message_in_receive_context_via_receive_queue()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            // cleanup
            await TestUtility.Delete("sales", "myqueue");

            var completed = new AsyncAutoResetEvent(false);

            // setting up the environment
            var container = new TransportPartsContainer();
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.SendViaReceiveQueue(true);

            // setup a basic topologySectionManager for testing
            var topology = await SetupEndpointOrientedTopology(container, "sales", settings);

            // setup the receive side of things
            var topologyOperator = (IOperateTopology)container.Resolve(typeof(TopologyOperator));
            var pump = new MessagePump(topology, topologyOperator);

            // setup the dispatching side of things
            var dispatcher = (IDispatchMessages)container.Resolve(typeof(IDispatchMessages));

            // create the destination queue
            var namespaceLifeCycle = (IManageNamespaceManagerLifeCycle)container.Resolve(typeof(IManageNamespaceManagerLifeCycle));
            var creator = (ICreateAzureServiceBusQueues)container.Resolve(typeof(ICreateAzureServiceBusQueues));
            var namespaceManager = namespaceLifeCycle.Get("namespaceName");
            await creator.Create("myqueue", namespaceManager);

            // setup the test
            var received = false;

            // Dummy CriticalError
            var criticalError = new CriticalError(ctx => TaskEx.Completed);

            await pump.Init(async context =>
            {
                // normally the core would do that
                context.Context.Set(context.TransportTransaction);

                received = true;

                var bytes = Encoding.UTF8.GetBytes("Whatever");
                var outgoingMessage = new OutgoingMessage("Id-1", new Dictionary<string, string>(), bytes);

                var transportOperations = new TransportOperations(new TransportOperation(outgoingMessage, new UnicastAddressTag("myqueue"), DispatchConsistency.Default, Enumerable.Empty<DeliveryConstraint>().ToList()));

                await dispatcher.Dispatch(transportOperations, new TransportTransaction(), context.Context); // makes sure the context propagates

                completed.Set();
            }, null, criticalError, new PushSettings("sales", "error", false, TransportTransactionMode.SendsAtomicWithReceive));
            
            // start the pump
            pump.Start(new PushRuntimeSettings(1));

            // send message to local queue
            var senderFactory = (MessageSenderCreator)container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create("sales", null, "namespaceName");
            await sender.Send(new BrokeredMessage
            {
                MessageId = "id-init"
            });

            await completed.WaitAsync(cts.Token);

            await Task.Delay(TimeSpan.FromSeconds(3)); // allow message count to update on queue

            // validate
            Assert.IsTrue(received);

            // check destination queue for dispatched message
            var queue = await namespaceManager.GetQueue("myqueue");
            var count = queue.MessageCount;
            Assert.IsTrue(count == 1, "'myqueue' was expected to have 1 message, but it had " + count + " instead");

            await pump.Stop();
        }

        [Test]
        public async Task Should_retry_after_rollback_in_less_then_thirty_seconds_when_using_via_queue()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            // cleanup
            await TestUtility.Delete("sales", "myqueue");

            var retried = new AsyncAutoResetEvent(false);
            var invocationCount = 0;
            var firstTime = DateTime.MinValue;
            var secondTime = DateTime.MaxValue;

            // setting up the environment
            var container = new TransportPartsContainer();
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.SendViaReceiveQueue(true);

            // setup a basic topologySectionManager for testing
            var topology = await SetupEndpointOrientedTopology(container, "sales", settings);

            // setup the receive side of things
            var topologyOperator = (IOperateTopology)container.Resolve(typeof(TopologyOperator));
            var pump = new MessagePump(topology, topologyOperator);

            // setup the dispatching side of things
            var dispatcher = (IDispatchMessages)container.Resolve(typeof(IDispatchMessages));

            // create the destination queue
            var namespaceLifeCycle = (IManageNamespaceManagerLifeCycle)container.Resolve(typeof(IManageNamespaceManagerLifeCycle));
            var creator = (ICreateAzureServiceBusQueues)container.Resolve(typeof(ICreateAzureServiceBusQueues));
            var namespaceManager = namespaceLifeCycle.Get("namespaceName");
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
            var criticalError = new CriticalError(ctx => TaskEx.Completed);

            await pump.Init(async context =>
            {
                // normally the core would do that
                context.Context.Set(context.TransportTransaction);

                var bytes = Encoding.UTF8.GetBytes("Whatever");
                var outgoingMessage = new OutgoingMessage("Id-1", new Dictionary<string, string>(), bytes);

                var transportOperations = new TransportOperations(new TransportOperation(outgoingMessage, new UnicastAddressTag("myqueue"), DispatchConsistency.Default, Enumerable.Empty<DeliveryConstraint>().ToList()));

                await dispatcher.Dispatch(transportOperations, new TransportTransaction(), context.Context);

                throw new Exception("Something bad happens");

            }, null, criticalError, new PushSettings("sales", "error", false, TransportTransactionMode.SendsAtomicWithReceive));


            // start the pump
            pump.Start(new PushRuntimeSettings(1));

            // send message to local queue
            var senderFactory = (MessageSenderCreator)container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create("sales", null, "namespaceName");
            await sender.Send(new BrokeredMessage());

            await Task.WhenAny(retried.WaitAsync(cts.Token).IgnoreCancellation());

            // stop the pump so retries don't keep going
            await pump.Stop();

            // validate
            var elapsed = secondTime - firstTime;
            Console.WriteLine("elapsed" + elapsed.TotalSeconds);
            Assert.IsTrue(elapsed < TimeSpan.FromSeconds(29)); // 29 instead of 30 to accommodate for a little clock drift
        }

        async Task<ITopologySectionManager> SetupEndpointOrientedTopology(TransportPartsContainer container, string enpointname, SettingsHolder settings)
        {
            container.Register(typeof(SettingsHolder), () => settings);
            settings.SetDefault("NServiceBus.Routing.EndpointName", enpointname);
            settings.Set<Conventions>(new Conventions());

            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.NamespacePartitioning().AddNamespace("namespaceName", AzureServiceBusConnectionString.Value);

            var topology = new EndpointOrientedTopology(container);
            topology.Initialize(settings);

            // create the topologySectionManager
            var topologyCreator = (ICreateTopology)container.Resolve(typeof(TopologyCreator));
            var sectionManager = container.Resolve<ITopologySectionManager>();
            await topologyCreator.Create(sectionManager.DetermineResourcesToCreate(new QueueBindings()));
            return sectionManager;
        }
    }
}
