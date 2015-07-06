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
            var expectedFilter = 
                string.Format("[NServiceBus.EnclosedMessageTypes] LIKE '{0}%' OR [NServiceBus.EnclosedMessageTypes] LIKE '%{0}%' OR [NServiceBus.EnclosedMessageTypes] LIKE '%{0}' OR [NServiceBus.EnclosedMessageTypes] = '{0}'", 
                eventType.FullName);
            Assert.AreEqual(filter, expectedFilter);
        }
    }

    public class SomeEvent : IEvent
    {
    }
}