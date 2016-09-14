namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Seam
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Threading.Tasks;
    using AzureServiceBus;
    using Transport.AzureServiceBus;
    using Settings;
    using Transport;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_message_pump_is_failing_to_receive_messages
    {
        [Test]
        public async Task Should_trigger_circuit_breaker()
        {
            var container = new TransportPartsContainer();

            var fakeTopologyOperator = new FakeTopologyOperator();
            container.Register<IOperateTopology>(() => fakeTopologyOperator);

            var settings = new SettingsHolder();
            new DefaultConfigurationValues().Apply(settings);
            container.Register<ReadOnlySettings>(() => settings);

            Exception exceptionReceivedByCircuitBreaker = null;
            var criticalErrorWasRaised = false;
            var stopwatch = new Stopwatch();

            // setup critical error action to capture exception thrown by message pump
            var criticalError = new CriticalError(ctx =>
            {
                stopwatch.Stop();
                criticalErrorWasRaised = true;
                exceptionReceivedByCircuitBreaker = ctx.Exception;
                return TaskEx.Completed;
            });
            criticalError.GetType().GetMethod("SetEndpoint", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Invoke(criticalError, new[] { new FakeEndpoint() });

            var pump = new MessagePump(new FakeTopology(), container);
            await pump.Init(context => TaskEx.Completed, null, criticalError, new PushSettings("sales", "error", false, TransportTransactionMode.ReceiveOnly));
            pump.OnError(exception =>
            {
                // circuit breaker is armed now
                stopwatch.Start();
                return TaskEx.Completed;
            });
            pump.Start(new PushRuntimeSettings(1));

            await fakeTopologyOperator.onIncomingMessage(new IncomingMessageDetails("id", new Dictionary<string, string>(), new byte[0]), new FakeReceiveContext());
            var exceptionThrownByMessagePump = new Exception("kaboom");
            await fakeTopologyOperator.onError(exceptionThrownByMessagePump);

            await Task.Delay(TimeSpan.FromSeconds(32)); // let circuit breaker kick in

            // validate
            Assert.IsTrue(criticalErrorWasRaised, "Expected critical error to be raised, but it wasn't");
            Assert.AreEqual(exceptionThrownByMessagePump, exceptionReceivedByCircuitBreaker, "Exception circuit breaker got should be the same as the one raised by message pump");
            Assert.That(stopwatch.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(TimeSpan.FromSeconds(30).TotalMilliseconds));
        }

        class FakeReceiveContext : ReceiveContext
        {
        }

        class FakeTopology : ITopologySectionManager
        {
            public TopologySection DetermineReceiveResources(string inputQueue)
            {
                return new TopologySection
                {
                    Namespaces = new List<RuntimeNamespaceInfo> { new RuntimeNamespaceInfo("name", ConnectionStringValue.Sample) },
                    Entities = new List<EntityInfo>()
                };
            }

            public TopologySection DetermineResourcesToCreate(QueueBindings queueBindings)
            {
                return new TopologySection();
            }

            public TopologySection DeterminePublishDestination(Type eventType)
            {
                return new TopologySection();
            }

            public TopologySection DetermineSendDestination(string destination)
            {
                return new TopologySection();
            }

            public TopologySection DetermineResourcesToSubscribeTo(Type eventType)
            {
                return new TopologySection();
            }

            public TopologySection DetermineResourcesToUnsubscribeFrom(Type eventtype)
            {
                return new TopologySection();
            }
        }

        class FakeTopologyOperator : IOperateTopology
        {
            public Func<Exception, Task> onError;
            public Func<IncomingMessageDetails, ReceiveContext, Task> onIncomingMessage;

            public void Start(TopologySection topology, int maximumConcurrency)
            {

            }

            public Task Stop()
            {
                return TaskEx.Completed;
            }

            public void Start(IEnumerable<EntityInfo> subscriptions)
            {

            }

            public Task Stop(IEnumerable<EntityInfo> subscriptions)
            {
                return TaskEx.Completed;
            }

            public void OnIncomingMessage(Func<IncomingMessageDetails, ReceiveContext, Task> func)
            {
                onIncomingMessage = func;
            }

            public void OnError(Func<Exception, Task> func)
            {
                onError = func;
            }

            public void OnProcessingFailure(Func<ErrorContext, Task<ErrorHandleResult>> func)
            {

            }
        }

        class FakeEndpoint : IEndpointInstance
        {
            public Task Stop()
            {
                return TaskEx.Completed;
            }

            public Task Send(object message, SendOptions options)
            {
                throw new NotImplementedException();
            }

            public Task Send<T>(Action<T> messageConstructor, SendOptions options)
            {
                throw new NotImplementedException();
            }

            public Task Publish(object message, PublishOptions options)
            {
                throw new NotImplementedException();
            }

            public Task Publish<T>(Action<T> messageConstructor, PublishOptions publishOptions)
            {
                throw new NotImplementedException();
            }

            public Task Subscribe(Type eventType, SubscribeOptions options)
            {
                throw new NotImplementedException();
            }

            public Task Unsubscribe(Type eventType, UnsubscribeOptions options)
            {
                throw new NotImplementedException();
            }
        }
    }
}