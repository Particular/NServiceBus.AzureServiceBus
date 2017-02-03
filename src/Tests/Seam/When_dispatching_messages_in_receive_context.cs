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
            completed = new AsyncAutoResetEvent(false);
            container = new TransportPartsContainer();
            settings = new SettingsHolder();
            settings.Set("NServiceBus.SharedQueue", SourceQueueName);
            criticalError = new CriticalError(ctx => TaskEx.Completed);

            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.SendViaReceiveQueue(true);

            SetupAsync(container, settings).GetAwaiter().GetResult();
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
            var namespaceManagerCreator = new NamespaceManagerCreator(settings);

            var namespaceLifecycleManager = new NamespaceManagerLifeCycleManagerInternal(namespaceManagerCreator);
            var topologyCreator = new TopologyCreator(new AzureServiceBusSubscriptionCreatorV6(endpointOrientedTopology.Settings.SubscriptionSettings, settings), 
                new AzureServiceBusQueueCreator(endpointOrientedTopology.Settings.QueueSettings,settings),  new AzureServiceBusTopicCreator(endpointOrientedTopology.Settings.TopicSettings), 
                namespaceLifecycleManager);
            var topologySectionManager = new EndpointOrientedTopologySectionManager(settings, container);
            await topologyCreator.Create(topologySectionManager.DetermineResourcesToCreate(new QueueBindings())).ConfigureAwait(false);

            // create the destination queue
            var creator = new AzureServiceBusQueueCreator(endpointOrientedTopology.Settings.QueueSettings, settings);
            namespaceManager = namespaceLifecycleManager.Get("namespaceName");
            await creator.Create(DestinationQueueName, namespaceManager).ConfigureAwait(false);

            dispatcher = endpointOrientedTopology.GetDispatcherFactory()();

            timeToWaitBeforeTriggeringTheCircuitBreaker = TimeSpan.FromSeconds(5);
            var messagingFactoryCreator = new MessagingFactoryCreator(namespaceLifecycleManager, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var receiverCreator = new MessageReceiverCreator(messagingFactoryLifeCycleManager, settings);
            var receiversLifeCycleManager = new MessageReceiverLifeCycleManager(receiverCreator, settings);
            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings, new DefaultConnectionStringToNamespaceAliasMapper(settings));
            pump = new MessagePump(new TopologyOperator(receiversLifeCycleManager, converter, settings), receiversLifeCycleManager, converter, topologySectionManager, settings, timeToWaitBeforeTriggeringTheCircuitBreaker);
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
        AsyncAutoResetEvent completed;
        MessagePump pump;
        IDispatchMessages dispatcher;
        CriticalError criticalError;
        TransportPartsContainer container;
        INamespaceManagerInternal namespaceManager;
        TimeSpan timeToWaitBeforeTriggeringTheCircuitBreaker;
        SettingsHolder settings;
        const string DestinationQueueName = "myqueue";
        const string SourceQueueName = "sales";
    }
}