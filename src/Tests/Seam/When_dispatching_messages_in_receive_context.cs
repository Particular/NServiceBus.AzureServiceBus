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

#pragma warning disable 618
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
            var settings = new SettingsHolder();
            settings.Set("NServiceBus.SharedQueue", SourceQueueName);
            criticalError = new CriticalError(ctx => TaskEx.Completed);

            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.SendViaReceiveQueue(true);

            SetupAsync(container, settings).GetAwaiter().GetResult();

            dispatcher = (IDispatchMessages) container.Resolve(typeof(IDispatchMessages));

            timeToWaitBeforeTriggeringTheCircuitBreaker = TimeSpan.FromSeconds(5);
            pump = new MessagePump(topology, container, settings, timeToWaitBeforeTriggeringTheCircuitBreaker);
        }

        async Task SetupAsync(TransportPartsContainer container, SettingsHolder settings)
        {
            await TestUtility.Delete(SourceQueueName, DestinationQueueName);

            topology = await SetupEndpointOrientedTopology(container, SourceQueueName, settings);

            // create the destination queue
            var namespaceLifeCycle = (IManageNamespaceManagerLifeCycle) container.Resolve(typeof(IManageNamespaceManagerLifeCycle));
            var creator = (ICreateAzureServiceBusQueues) container.Resolve(typeof(ICreateAzureServiceBusQueues));
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

                    await dispatcher.Dispatch(transportOperations, context.TransportTransaction, context.Extensions); // makes sure the context propagates

                    completed.Set();
                }, null, criticalError, new PushSettings(SourceQueueName, "error", false, TransportTransactionMode.ReceiveOnly));

                // start the pump
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

                    await dispatcher.Dispatch(transportOperations, context.TransportTransaction, context.Extensions); // makes sure the context propagates

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
            var stringBuilder = new StringBuilder();
            try
            {
                var invocationCount = 0;

                Stopwatch stopWatch = null;

                await pump.Init(async context =>
                {
                    stringBuilder.AppendLine("Received " + invocationCount);
                    var bytes = Encoding.UTF8.GetBytes("Whatever");
                    var outgoingMessage = new OutgoingMessage("Id-1", new Dictionary<string, string>(), bytes);

                    var transportOperations = new TransportOperations(new TransportOperation(outgoingMessage, new UnicastAddressTag(DestinationQueueName), DispatchConsistency.Default, Enumerable.Empty<DeliveryConstraint>().ToList()));

                    await dispatcher.Dispatch(transportOperations, context.TransportTransaction, context.Extensions);

                    // doesn't need interlocked because concurrency is set to 1
                    invocationCount++;

                    if (invocationCount == 1)
                    {
                        stopWatch = Stopwatch.StartNew();

                        stringBuilder.AppendLine("Throwing " + invocationCount);
                        throw new Exception("Something bad happens");
                    }

                    // ReSharper disable once PossibleNullReferenceException
                    stopWatch.Stop();
                    stringBuilder.AppendLine("Completing " + invocationCount);
                    completed.Set();
                }, context => Task.FromResult(ErrorHandleResult.RetryRequired), criticalError, new PushSettings(SourceQueueName, "error", false, TransportTransactionMode.SendsAtomicWithReceive));

                // start the pump
                pump.Start(new PushRuntimeSettings(1));

                await SendEmptyMessageToLocalTestQueue();

                await WaitForCompletion();

                var queue = await GetTestQueue();

                var elapsed = stopWatch.Elapsed;

                Assert.IsTrue(queue.MessageCount == 1, "'myqueue' was expected to have 1 message, but it had " + queue.MessageCount + " instead");
                Assert.IsTrue(elapsed < timeToWaitBeforeTriggeringTheCircuitBreaker);
            }
            finally
            {
                // cleanup
                await pump.Stop();
                
                Console.WriteLine(stringBuilder);
            }
        }

        async Task<ITopologySectionManager> SetupEndpointOrientedTopology(TransportPartsContainer container, string endpointName, SettingsHolder settings)
        {
            container.Register(typeof(SettingsHolder), () => settings);
            settings.SetDefault("NServiceBus.Routing.EndpointName", endpointName);
            settings.Set<Conventions>(new Conventions());

            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.NamespacePartitioning().AddNamespace("namespaceName", AzureServiceBusConnectionString.Value);

            var endpointOrientedTopology = new EndpointOrientedTopology(container);
            endpointOrientedTopology.Initialize(settings);

            // create the topologySectionManager
            var topologyCreator = (ICreateTopology) container.Resolve(typeof(TopologyCreator));
            var sectionManager = container.Resolve<ITopologySectionManager>();
            await topologyCreator.Create(sectionManager.DetermineResourcesToCreate(new QueueBindings()));
            container.RegisterSingleton<TopologyOperator>();
            return sectionManager;
        }

        async Task SendEmptyMessageToLocalTestQueue()
        {
            // send message to local queue
            var senderFactory = (MessageSenderCreator) container.Resolve(typeof(MessageSenderCreator));
            var sender = await senderFactory.Create(SourceQueueName, null, "namespaceName");
            await sender.Send(new BrokeredMessage
            {
                MessageId = "id-init"
            });
        }

        async Task WaitForCompletion()
        {
            await completed.WaitAsync(tokenSource.Token).IgnoreCancellation();
            await Task.Delay(TimeSpan.FromSeconds(5)); // allow message count to update on queue
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
        ITopologySectionManager topology;
        INamespaceManager namespaceManager;
        TimeSpan timeToWaitBeforeTriggeringTheCircuitBreaker;
        const string DestinationQueueName = "myqueue";
        const string SourceQueueName = "sales";
    }
}