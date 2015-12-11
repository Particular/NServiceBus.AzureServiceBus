namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Seam
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading.Tasks;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.Transports;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_message_pump_is_failing_to_receive_messages
    {
        [Test]
        public async Task Should_trigger_circuit_braker()
        {
            var fakeTopologyOperator = new FakeTopologyOperator();
            Exception exceptionReceivedByCircuitBreaker = null;
            var criticalErrorWasRaised = false;
            var stopwatch = new Stopwatch();

            // setup critical error action to capture exception thrown by message pump
            var criticalError = new CriticalError((ei, error, exception) =>
            {
                stopwatch.Stop();
                criticalErrorWasRaised = true;
                exceptionReceivedByCircuitBreaker = exception;
                return Task.FromResult(0);
            });
            criticalError.GetType().GetProperty("Endpoint", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).SetValue(criticalError, new FakeEndpoint(), null);

            var pump = new MessagePump(new FakeTopology(), fakeTopologyOperator);
            pump.OnCriticalError(criticalError);
            pump.OnError(exception =>
            {
                // circuit breaker is armed now
                stopwatch.Start();
                return TaskEx.Completed;
            });

            pump.Init(context => Task.FromResult(0), new PushSettings("sales", "error", false, TransactionSupport.MultiQueue));
            pump.Start(new PushRuntimeSettings(1));

            await fakeTopologyOperator.onIncomingMessage(new IncomingMessageDetails("id", new Dictionary<string, string>(), new MemoryStream()), new BrokeredMessageReceiveContext());
            var exceptionThrownByMessagePump = new Exception("kaboom");
            await fakeTopologyOperator.onError(exceptionThrownByMessagePump);

            await Task.Delay(TimeSpan.FromSeconds(32)); // let circuit breaker kick in

            // validate
            Assert.IsTrue(criticalErrorWasRaised, "Expected critical error to be raised, but it wasn't");
            Assert.AreEqual(exceptionThrownByMessagePump, exceptionReceivedByCircuitBreaker, "Exception circuit breaker got should be the same as the one raised by message pump");
            Assert.That(stopwatch.ElapsedMilliseconds, Is.GreaterThanOrEqualTo(TimeSpan.FromSeconds(30).TotalMilliseconds));
        }

        private class FakeTopology : ITopologySectionManager
        {
            public TopologySection DetermineReceiveResources()
            {
                return new TopologySection();
            }

            public TopologySection DetermineResourcesToCreate()
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

        private class FakeTopologyOperator : IOperateTopology
        {
            public Func<Exception, Task> onError;
            public Func<IncomingMessageDetails, ReceiveContext, Task> onIncomingMessage;

            public void Start(TopologySection topology, int maximumConcurrency)
            {

            }

            public Task Stop()
            {
                return Task.FromResult(0);
            }

            public void Start(IEnumerable<EntityInfo> subscriptions)
            {

            }

            public Task Stop(IEnumerable<EntityInfo> subscriptions)
            {
                return Task.FromResult(0);
            }

            public void OnIncomingMessage(Func<IncomingMessageDetails, ReceiveContext, Task> func)
            {
                onIncomingMessage = func;
            }

            public void OnError(Func<Exception, Task> func)
            {
                onError = func;
            }
        }

        private class FakeEndpoint : IEndpointInstance
        {
            public IBusContext CreateBusContext()
            {
                return null;
            }

            public Task Stop()
            {
                return Task.FromResult(0);
            }
        }
    }
}