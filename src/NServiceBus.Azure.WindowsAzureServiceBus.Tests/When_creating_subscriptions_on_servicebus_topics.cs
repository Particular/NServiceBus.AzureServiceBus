namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests
{
    using Azure.Transports.WindowsAzureServiceBus;
    using NUnit.Framework;

    [TestFixture]
    [Category("Azure")]
    public class When_creating_subscriptions_on_servicebus_topics
    {
        [Test]
        public void Should_filter_on_subscribed_eventtype_somewhere_in_enclosed_messagetypes_header()
        {
            var eventType = typeof(SomeEvent);

            var filter = new ServicebusSubscriptionFilterBuilder().BuildFor(eventType);

            Assert.AreEqual(filter, "[NServiceBus.EnclosedMessageTypes] LIKE 'NServiceBus.Azure.WindowsAzureServiceBus.Tests.SomeEvent%' OR [NServiceBus.EnclosedMessageTypes] LIKE '%NServiceBus.Azure.WindowsAzureServiceBus.Tests.SomeEvent%' OR [NServiceBus.EnclosedMessageTypes] LIKE '%NServiceBus.Azure.WindowsAzureServiceBus.Tests.SomeEvent' OR [NServiceBus.EnclosedMessageTypes] = 'NServiceBus.Azure.WindowsAzureServiceBus.Tests.SomeEvent'");
        }
    }

    public class SomeEvent : IEvent
    {
    }
}