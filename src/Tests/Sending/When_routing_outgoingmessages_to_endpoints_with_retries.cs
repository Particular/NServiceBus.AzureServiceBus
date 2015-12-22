namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using FakeItEasy;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Azure.WindowsAzureServiceBus.Tests;
    using NServiceBus.DeliveryConstraints;
    using NServiceBus.Routing;
    using NServiceBus.Settings;
    using NServiceBus.Transports;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_routing_outgoingmessages_to_endpoints_with_retries
    {
        [Test]
        public void Should_use_cloned_original_brokered_messages_on_retries()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var messageSenderSettings = new AzureServiceBusMessageSenderSettings(settings).BackOffTimeOnThrottle(TimeSpan.Zero);
            messageSenderSettings.RetryAttemptsOnThrottle(1);

            var topology = A.Fake<ITopologySectionManager>();
            var clientLifecycleManager = A.Fake<IManageMessageSenderLifeCycle>();
            var messageSender = A.Fake<IMessageSender>();

            var usedMessages = new List<BrokeredMessage>();

            A.CallTo(() => topology.DetermineSendDestination(A<string>._))
                .Returns(new TopologySection
                {
                    Entities = new[]
                    {
                        new EntityInfo
                        {
                            Path = "MyQueue",
                            Namespace = new NamespaceInfo(AzureServiceBusConnectionString.Value, NamespaceMode.Active)
                        }
                    }
                });

            A.CallTo(() => clientLifecycleManager.Get(A<string>.Ignored, A<string>.Ignored, A<string>.Ignored))
                .Returns(messageSender);

            A.CallTo(() => messageSender.SendBatch(A<IEnumerable<BrokeredMessage>>._))
                .Invokes((IEnumerable<BrokeredMessage> received) => usedMessages.Add(received.First()))
                .Throws(new ServerBusyException("busy"));

            var router = new DefaultOutgoingBatchRouter(
                topology,
                new DefaultOutgoingMessagesToBrokeredMessagesConverter(settings), 
                clientLifecycleManager, settings);

            var outgoingMessage = new OutgoingMessage("SomeId", new Dictionary<string, string>(), new byte[] {});
            var dispatchOptions = new DispatchOptions(new UnicastAddressTag("MyQueue"), DispatchConsistency.Default, new List<DeliveryConstraint>());

            Assert.That(async () => await router.RouteBatch(new[] { new Tuple<OutgoingMessage, DispatchOptions>(outgoingMessage, dispatchOptions) }, new RoutingOptions ()), Throws.Exception);
            Assert.AreNotSame(usedMessages[0], usedMessages[1], "retried message should be a clone and not the original BrokeredMessage");
        }
    }
}