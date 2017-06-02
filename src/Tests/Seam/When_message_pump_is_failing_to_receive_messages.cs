namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Seam
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AzureServiceBus;
    using NUnit.Framework;
    using Settings;
    using Transport;
    using Transport.AzureServiceBus;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_message_pump_is_failing_to_receive_messages
    {
        [Test]
        public async Task Should_trigger_circuit_breaker()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            var container = new TransportPartsContainer();

            var fakeTopologyOperator = new FakeTopologyOperator();
            container.Register<IOperateTopologyInternal>(() => fakeTopologyOperator);

            var settings = new SettingsHolder();
            settings.Set("NServiceBus.SharedQueue", "sales");
            DefaultConfigurationValues.Apply(settings);
            container.Register<ReadOnlySettings>(() => settings);

            Exception exceptionReceivedByCircuitBreaker = null;
            var criticalErrorWasRaised = false;

            var tcs = new TaskCompletionSource<object>();
            cts.Token.Register(() => tcs.TrySetCanceled());

            // setup critical error action to capture exception thrown by message pump
            var criticalError = new FakeCriticalError(ctx =>
            {
                criticalErrorWasRaised = true;
                exceptionReceivedByCircuitBreaker = ctx.Exception;

                tcs.TrySetResult(null);

                return TaskEx.Completed;
            });

            var pump = new MessagePump(fakeTopologyOperator, null, null, new FakeTopologyManager(), settings, TimeSpan.FromSeconds(1));
            await pump.Init(context => TaskEx.Completed, null, criticalError, new PushSettings("sales", "error", false, TransportTransactionMode.ReceiveOnly));

            pump.Start(new PushRuntimeSettings(1));

            await fakeTopologyOperator.onIncomingMessage(new IncomingMessageDetailsInternal("id", new Dictionary<string, string>(), new byte[0]), new FakeReceiveContext());
            var exceptionThrownByMessagePump = new Exception("kaboom");
            await fakeTopologyOperator.onError(exceptionThrownByMessagePump);

            await tcs.Task; // let circuit breaker kick in

            // validate
            Assert.IsTrue(criticalErrorWasRaised, "Expected critical error to be raised, but it wasn't");
            Assert.AreEqual(exceptionThrownByMessagePump, exceptionReceivedByCircuitBreaker, "Exception circuit breaker got should be the same as the one raised by message pump");
        }

        class FakeCriticalError : CriticalError
        {
            Func<ICriticalErrorContext, Task> func;

            public FakeCriticalError(Func<ICriticalErrorContext, Task> onCriticalErrorAction) : base(onCriticalErrorAction)
            {
                func = onCriticalErrorAction;
            }

            public override void Raise(string errorMessage, Exception exception)
            {
                func(new CriticalErrorContext(() => TaskEx.Completed, errorMessage, exception)).GetAwaiter().GetResult();
            }
        }

        class FakeReceiveContext : ReceiveContextInternal
        {
        }

        class FakeTopologyManager : ITopologySectionManagerInternal
        {
            public TopologySectionInternal DetermineReceiveResources(string inputQueue)
            {
                return new TopologySectionInternal
                {
                    Namespaces = new List<RuntimeNamespaceInfo>
                    {
                        new RuntimeNamespaceInfo("name", ConnectionStringValue.Sample)
                    },
                    Entities = new List<EntityInfoInternal>()
                };
            }

            public Task<TopologySectionInternal> DetermineResourcesToCreate(QueueBindings queueBindings)
            {
                return Task.FromResult(new TopologySectionInternal());
            }

            public Task<TopologySectionInternal> DeterminePublishDestination(Type eventType)
            {
                return Task.FromResult(new TopologySectionInternal());
            }

            public TopologySectionInternal DetermineSendDestination(string destination)
            {
                return new TopologySectionInternal();
            }

            public Task<TopologySectionInternal> DetermineResourcesToSubscribeTo(Type eventType)
            {
                return Task.FromResult(new TopologySectionInternal());
            }

            public TopologySectionInternal DetermineResourcesToUnsubscribeFrom(Type eventType)
            {
                return new TopologySectionInternal();
            }
        }

        class FakeTopologyOperator : IOperateTopologyInternal
        {
            public void Start(TopologySectionInternal topology, int maximumConcurrency)
            {
            }

            public Task Stop()
            {
                return TaskEx.Completed;
            }

            public void Start(IEnumerable<EntityInfoInternal> subscriptions)
            {
            }

            public Task Stop(IEnumerable<EntityInfoInternal> subscriptions)
            {
                return TaskEx.Completed;
            }

            public void OnIncomingMessage(Func<IncomingMessageDetailsInternal, ReceiveContextInternal, Task> func)
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

            public Func<Exception, Task> onError;
            public Func<IncomingMessageDetailsInternal, ReceiveContextInternal, Task> onIncomingMessage;
        }
    }
}