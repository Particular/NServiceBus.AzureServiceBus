namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Seam
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AzureServiceBus;
    using DeliveryConstraints;
    using Microsoft.ServiceBus.Messaging;
    using NUnit.Framework;
    using Routing;
    using Settings;
    using TestUtils;
    using Transport;
    using Transport.AzureServiceBus;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_dispatching_messages_in_receive_context
    {
        [SetUp]
        public void SetUp()
        {
            tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            completed = new AsyncManualResetEvent(false);
            container = new TransportPartsContainer();
            settings = new SettingsHolder();
            settings.Set("NServiceBus.SharedQueue", SourceQueueName);
            criticalError = new CriticalError(ctx => TaskEx.Completed);

            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.SendViaReceiveQueue(true);

            SetupAsync(container, settings).GetAwaiter().GetResult();

            dispatcher = (IDispatchMessages) container.Resolve(typeof(IDispatchMessages));

            timeToWaitBeforeTriggeringTheCircuitBreaker = TimeSpan.FromSeconds(5);
            var clientEntities = new MessageReceiverLifeCycleManager(new MessageReceiverCreator(new MessagingFactoryLifeCycleManager(new MessagingFactoryCreator(new NamespaceManagerLifeCycleManagerInternal(new NamespaceManagerCreator(settings)), settings), settings), settings), settings);
            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings, new DefaultConnectionStringToNamespaceAliasMapper(settings));
            pump = new MessagePump(new TopologyOperator(clientEntities, converter, settings), clientEntities, converter, sectionManager, settings, timeToWaitBeforeTriggeringTheCircuitBreaker);
        }

        async Task SetupAsync(TransportPartsContainer container, SettingsHolder settings)
        {
            await TestUtility.Delete(SourceQueueName, DestinationQueueName);

            container.Register(typeof(SettingsHolder), () => settings);
            settings.SetDefault("NServiceBus.Routing.EndpointName", SourceQueueName);
            settings.Set<Conventions>(new Conventions());

            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.NamespacePartitioning().AddNamespace("namespaceName", AzureServiceBusConnectionString.Value);

            var endpointOrientedTopology = new EndpointOrientedTopologyInternal(container);
            endpointOrientedTopology.Initialize(settings);

            // create the topologySectionManager
            var topologyCreator = (ICreateTopologyInternal) container.Resolve(typeof(TopologyCreator));
            sectionManager = container.Resolve<ITopologySectionManagerInternal>();
            await topologyCreator.Create(sectionManager.DetermineResourcesToCreate(new QueueBindings()));
            container.RegisterSingleton<TopologyOperator>();

            // create the destination queue
            var namespaceLifeCycle = new NamespaceManagerLifeCycleManagerInternal(new NamespaceManagerCreator(settings));
            var creator = new AzureServiceBusQueueCreator(endpointOrientedTopology.Settings.QueueSettings, settings);
            namespaceManager = namespaceLifeCycle.Get("namespaceName");
            await creator.Create(DestinationQueueName, namespaceManager);
        }

        [Test]
        public async Task Should_dispatch_message_in_receive_context()
        {
            try
            {
                // setup the test
                var received = false;

                await pump.Init(async context =>
                {
                    received = true;

                    var bytes = Encoding.UTF8.GetBytes("Whatever");
                    var outgoingMessage = new OutgoingMessage("Id-1", new Dictionary<string, string>(), bytes);

                    var transportOperations = new TransportOperations(new TransportOperation(outgoingMessage, new UnicastAddressTag(DestinationQueueName), DispatchConsistency.Default, Enumerable.Empty<DeliveryConstraint>().ToList()));

                    await dispatcher.Dispatch(transportOperations, context.TransportTransaction, context.Context); // makes sure the context propagates

                    completed.Set();
                }, null, criticalError, new PushSettings(SourceQueueName, "error", false, TransportTransactionMode.ReceiveOnly));

                // start the pump
                // start the pump
                pump.Start(new PushRuntimeSettings(1));

                await SendEmptyMessageToLocalTestQueue();

                await WaitForCompletion();

                var queue = await GetTestQueue();

                Assert.IsTrue(received);
                Assert.IsTrue(queue.MessageCount == 1, "'myqueue' was expected to have 1 message, but it didn't");
            }
            finally
            {
                // cleanup
                await pump.Stop();
            }
        }

        [Test]
        public async Task Should_dispatch_message_in_receive_context_via_receive_queue()
        {
            try
            {
                var received = false;

                await pump.Init(async context =>
                {
                    received = true;

                    var bytes = Encoding.UTF8.GetBytes("Whatever");
                    var outgoingMessage = new OutgoingMessage("Id-1", new Dictionary<string, string>(), bytes);

                    var transportOperations = new TransportOperations(new TransportOperation(outgoingMessage, new UnicastAddressTag(DestinationQueueName), DispatchConsistency.Default, Enumerable.Empty<DeliveryConstraint>().ToList()));

                    await dispatcher.Dispatch(transportOperations, context.TransportTransaction, context.Context); // makes sure the context propagates

                    completed.Set();
                }, null, criticalError, new PushSettings(SourceQueueName, "error", false, TransportTransactionMode.SendsAtomicWithReceive));

                // start the pump
                pump.Start(new PushRuntimeSettings(1));

                await SendEmptyMessageToLocalTestQueue();

                await WaitForCompletion();

                var queue = await GetTestQueue();

                Assert.IsTrue(received);
                Assert.IsTrue(queue.MessageCount == 1, "'myqueue' was expected to have 1 message, but it had " + queue.MessageCount + " instead");
            }
            finally
            {
                // cleanup
                await pump.Stop();
            }
        }

        [Test]
        public async Task Should_retry_after_rollback_in_less_than_the_default_circuit_breaker_interval_when_using_via_queue()
        {
            try
            {
                var invocationCount = 0;

                Stopwatch stopWatch = null;

                await pump.Init(async context =>
                {
                    var bytes = Encoding.UTF8.GetBytes("Whatever");
                    var outgoingMessage = new OutgoingMessage("Id-1", new Dictionary<string, string>(), bytes);

                    var transportOperations = new TransportOperations(new TransportOperation(outgoingMessage, new UnicastAddressTag(DestinationQueueName), DispatchConsistency.Default, Enumerable.Empty<DeliveryConstraint>().ToList()));

                    await dispatcher.Dispatch(transportOperations, context.TransportTransaction, context.Context);

                    // doesn't need interlocked because concurrency is set to 1
                    invocationCount++;

                    if (invocationCount == 1)
                    {
                        stopWatch = Stopwatch.StartNew();

                        throw new Exception("Something bad happens");
                    }

                    // ReSharper disable once PossibleNullReferenceException
                    stopWatch.Stop();
                    completed.Set();
                }, null, criticalError, new PushSettings(SourceQueueName, "error", false, TransportTransactionMode.SendsAtomicWithReceive));

                // start the pump
                pump.Start(new PushRuntimeSettings(1));

                await SendEmptyMessageToLocalTestQueue();

                await WaitForCompletion();

                var queue = await GetTestQueue();

                var elapsed = stopWatch.Elapsed;

                Assert.IsTrue(queue.MessageCount == 1, "'myqueue' was expected to have 1 message, but it didn't");
                Assert.IsTrue(elapsed < timeToWaitBeforeTriggeringTheCircuitBreaker);
            }
            finally
            {
                // cleanup
                await pump.Stop();
            }
        }

        async Task SendEmptyMessageToLocalTestQueue()
        {
            // send message to local queue
            var senderFactory = new MessageSenderCreator(new MessagingFactoryLifeCycleManager(new MessagingFactoryCreator(new NamespaceManagerLifeCycleManagerInternal(new NamespaceManagerCreator(settings)), settings), settings), settings);
            var sender = await senderFactory.Create(SourceQueueName, null, "namespaceName");
            await sender.Send(new BrokeredMessage
            {
                MessageId = "id-init"
            });
        }

        async Task WaitForCompletion()
        {
            await completed.WaitAsync(tokenSource.Token).IgnoreCancellation();
            await Task.Delay(TimeSpan.FromSeconds(3)); // allow message count to update on queue
        }

        Task<QueueDescription> GetTestQueue()
        {
            return namespaceManager.GetQueue(DestinationQueueName);
        }

        CancellationTokenSource tokenSource;
        AsyncManualResetEvent completed;
        MessagePump pump;
        IDispatchMessages dispatcher;
        CriticalError criticalError;
        TransportPartsContainer container;
        ITopologySectionManagerInternal sectionManager;
        INamespaceManagerInternal namespaceManager;
        TimeSpan timeToWaitBeforeTriggeringTheCircuitBreaker;
        SettingsHolder settings;
        const string DestinationQueueName = "myqueue";
        const string SourceQueueName = "sales";
    }
}