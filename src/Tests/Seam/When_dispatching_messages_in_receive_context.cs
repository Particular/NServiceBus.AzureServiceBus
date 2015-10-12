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
    using NServiceBus.ConsistencyGuarantees;
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
            var container = new FuncBuilder();
            var settings = new SettingsHolder();

            // setup a basic topology for testing
            var topology = await SetupBasicTopology(container, "sales", settings);

            // setup the receive side of things
            var topologyOperator = (IOperateTopology)container.Build(typeof(TopologyOperator));
            var pump = new MessagePump(topology, topologyOperator);
            
            // setup the dispatching side of things
            var clientLifecycleManager = (IManageMessageSenderLifeCycle)container.Build(typeof(IManageMessageSenderLifeCycle));
            var router = new DefaultOutgoingMessageRouter(topology, new DefaultOutgoingMessagesToBrokeredMessagesConverter(settings), clientLifecycleManager, settings);
            var dispatcher = new Dispatcher(router, settings);

            // create the destination queue
            var namespaceLifeCycle = (IManageNamespaceManagerLifeCycle)container.Build(typeof(IManageNamespaceManagerLifeCycle));
            var creator = (ICreateAzureServiceBusQueues)container.Build(typeof(ICreateAzureServiceBusQueues));
            var namespaceManager = namespaceLifeCycle.Get(AzureServiceBusConnectionString.Value);
            await creator.CreateAsync("myqueue", namespaceManager);

            // setup the test
            var received = false;

            pump.Init(async context =>
            {
                received = true;

                var bytes = Encoding.UTF8.GetBytes("Whatever");
                var outgoingMessage = new OutgoingMessage("Id-1", new Dictionary<string, string>(), bytes);
                var dispatchOptions = new DispatchOptions(new DirectToTargetDestination("myqueue"), DispatchConsistency.Default, Enumerable.Empty<DeliveryConstraint>());

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

            }, new PushSettings("sales", "error", false, ConsistencyGuarantee.AtLeastOnce));

            // start the pump
            pump.Start(new PushRuntimeSettings(1));

            // send message to local queue
            var senderFactory = (MessageSenderCreator)container.Build(typeof(MessageSenderCreator));
            var sender = await senderFactory.CreateAsync("sales", null, AzureServiceBusConnectionString.Value);
            await sender.SendAsync(new BrokeredMessage());

            await completed.WaitOne();

            await Task.Delay(TimeSpan.FromSeconds(1)); //the OnCompleted callbacks are called right before the batch is completed, so give it a second to do that

            // validate
            Assert.IsTrue(received);

            // check destination queue for dispatched message
            var queue = await namespaceManager.GetQueueAsync("myqueue");
            Assert.IsTrue(queue.MessageCount == 1);

            // cleanup 
            await pump.Stop();

            await Cleanup(container, "sales", "myqueue");
        }

        [Test]
        public async Task Should_dispatch_message_in_receive_context_via_receive_queue()
        {
            var completed = new AsyncAutoResetEvent(false);

            // setting up the environment
            var container = new FuncBuilder();
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.Connectivity().SendViaReceiveQueue(true);

            // setup a basic topology for testing
            var topology = await SetupBasicTopology(container, "sales", settings);

            // setup the receive side of things
            var topologyOperator = (IOperateTopology)container.Build(typeof(TopologyOperator));
            var pump = new MessagePump(topology, topologyOperator);

            // setup the dispatching side of things
            var clientLifecycleManager = (IManageMessageSenderLifeCycle)container.Build(typeof(IManageMessageSenderLifeCycle));
            var router = new DefaultOutgoingMessageRouter(topology, new DefaultOutgoingMessagesToBrokeredMessagesConverter(settings), clientLifecycleManager, settings);
            var dispatcher = new Dispatcher(router, settings);

            // create the destination queue
            var namespaceLifeCycle = (IManageNamespaceManagerLifeCycle)container.Build(typeof(IManageNamespaceManagerLifeCycle));
            var creator = (ICreateAzureServiceBusQueues)container.Build(typeof(ICreateAzureServiceBusQueues));
            var namespaceManager = namespaceLifeCycle.Get(AzureServiceBusConnectionString.Value);
            await creator.CreateAsync("myqueue", namespaceManager);

            // setup the test
            var received = false;

            pump.Init(async context =>
            {
                received = true;

                var bytes = Encoding.UTF8.GetBytes("Whatever");
                var outgoingMessage = new OutgoingMessage("Id-1", new Dictionary<string, string>(), bytes);
                var dispatchOptions = new DispatchOptions(new DirectToTargetDestination("myqueue"), DispatchConsistency.Default, Enumerable.Empty<DeliveryConstraint>());

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

            }, new PushSettings("sales", "error", false, ConsistencyGuarantee.AtLeastOnce));

            // start the pump
            pump.Start(new PushRuntimeSettings(1));

            // send message to local queue
            var senderFactory = (MessageSenderCreator)container.Build(typeof(MessageSenderCreator));
            var sender = await senderFactory.CreateAsync("sales", null, AzureServiceBusConnectionString.Value);
            await sender.SendAsync(new BrokeredMessage());

            await completed.WaitOne();

            await Task.Delay(TimeSpan.FromSeconds(1)); //the OnCompleted callbacks are called right before the batch is completed, so give it a second to do that

            // validate
            Assert.IsTrue(received);

            // check destination queue for dispatched message
            var queue = await namespaceManager.GetQueueAsync("myqueue");
            Assert.IsTrue(queue.MessageCount == 1);

            // cleanup 
            await pump.Stop();

            await Cleanup(container, "sales", "myqueue");
        }

        static async Task Cleanup(FuncBuilder container, params string[] endpointnames)
        {
            var namespaceLifeCycle = (IManageNamespaceManagerLifeCycle)container.Build(typeof(IManageNamespaceManagerLifeCycle));
            var namespaceManager = namespaceLifeCycle.Get(AzureServiceBusConnectionString.Value);

            foreach (var endpointname in endpointnames)
            {
                await namespaceManager.DeleteQueueAsync(endpointname);
            }
        }

        async Task<BasicTopology> SetupBasicTopology(FuncBuilder container, string enpointname, SettingsHolder settings)
        {
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