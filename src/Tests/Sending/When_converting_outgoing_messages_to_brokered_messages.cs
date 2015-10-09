namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using NServiceBus.DelayedDelivery;
    using NServiceBus.DeliveryConstraints;
    using NServiceBus.Routing;
    using NServiceBus.Settings;
    using NServiceBus.Transports;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_converting_outgoing_messages_to_brokered_messages
    {
        [Test]
        public void Should_inject_body_as_byte_array_by_default()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var converter = new DefaultOutgoingMessagesToBrokeredMessagesConverter(settings);

            var bytes = Encoding.UTF8.GetBytes("Whatever");

            var outgoingMessage = new OutgoingMessage("SomeId", new Dictionary<string, string>(), bytes);
            var dispatchOptions = new DispatchOptions(new DirectToTargetDestination("MyQueue"), DispatchConsistency.Default, new List<DeliveryConstraint>());

            var brokeredMessage = converter.Convert(outgoingMessage, dispatchOptions);

            var body = Encoding.UTF8.GetString(brokeredMessage.GetBody<byte[]>());

            Assert.AreEqual(body, "Whatever");
        }

        [Test]
        public void Should_extract_body_as_stream_when_configured()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Serialization().BrokeredMessageBodyType(SupportedBrokeredMessageBodyTypes.Stream);

            var converter = new DefaultOutgoingMessagesToBrokeredMessagesConverter(settings);

            var bytes = Encoding.UTF8.GetBytes("Whatever");

            var outgoingMessage = new OutgoingMessage("SomeId", new Dictionary<string, string>(), bytes);
            var dispatchOptions = new DispatchOptions(new DirectToTargetDestination("MyQueue"), DispatchConsistency.Default, new List<DeliveryConstraint>());

            var brokeredMessage = converter.Convert(outgoingMessage, dispatchOptions);

            var sr = new StreamReader(brokeredMessage.GetBody<Stream>());
            var body = sr.ReadToEnd();

            Assert.AreEqual("Whatever", body);
        }

        [Test]
        public void Should_copy_the_message_id()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var converter = new DefaultOutgoingMessagesToBrokeredMessagesConverter(settings);

            var outgoingMessage = new OutgoingMessage("SomeId", new Dictionary<string, string>(), new byte[0]);
            var dispatchOptions = new DispatchOptions(new DirectToTargetDestination("MyQueue"), DispatchConsistency.Default, new List<DeliveryConstraint>());

            var brokeredMessage = converter.Convert(outgoingMessage, dispatchOptions);

            Assert.IsTrue(brokeredMessage.MessageId == "SomeId");
        }

        [Test]
        public void Should_copy_the_headers()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var converter = new DefaultOutgoingMessagesToBrokeredMessagesConverter(settings);

            var headers = new Dictionary<string, string>
            {
                {"MyHeader", "MyValue"}
            };

            var outgoingMessage = new OutgoingMessage("SomeId", headers, new byte[0]);
            var dispatchOptions = new DispatchOptions(new DirectToTargetDestination("MyQueue"), DispatchConsistency.Default, new List<DeliveryConstraint>());

            var brokeredMessage = converter.Convert(outgoingMessage, dispatchOptions);

            Assert.IsTrue(brokeredMessage.Properties.ContainsKey("MyHeader"));
            Assert.AreEqual("MyValue", brokeredMessage.Properties["MyHeader"]);
        }

        [Test]
        public void Should_apply_delayed_delivery()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var converter = new DefaultOutgoingMessagesToBrokeredMessagesConverter(settings);

            var now = DateTime.UtcNow;
            Time.UtcNow = () => now ;

            var delay = new DelayDeliveryWith(TimeSpan.FromDays(1));

            var outgoingMessage = new OutgoingMessage("SomeId", new Dictionary<string, string>(), new byte[0]);
            var dispatchOptions = new DispatchOptions(new DirectToTargetDestination("MyQueue"), DispatchConsistency.Default, new List<DeliveryConstraint>() { delay });

            var brokeredMessage = converter.Convert(outgoingMessage, dispatchOptions);

            Assert.IsTrue(brokeredMessage.ScheduledEnqueueTimeUtc == now.AddDays(1));

            Time.UtcNow = () => DateTime.UtcNow;
        }

        [Test]
        public void Should_apply_delivery_at_specific_date()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var converter = new DefaultOutgoingMessagesToBrokeredMessagesConverter(settings);

            var now = DateTime.UtcNow;
            Time.UtcNow = () => now;
            var delay = new DoNotDeliverBefore(now.AddDays(2));

            var outgoingMessage = new OutgoingMessage("SomeId", new Dictionary<string, string>(), new byte[0]);
            var dispatchOptions = new DispatchOptions(new DirectToTargetDestination("MyQueue"), DispatchConsistency.Default, new List<DeliveryConstraint>() { delay });

            var brokeredMessage = converter.Convert(outgoingMessage, dispatchOptions);

            Assert.IsTrue(brokeredMessage.ScheduledEnqueueTimeUtc == now.AddDays(2));

            Time.UtcNow = () => DateTime.UtcNow;
        }

        [Test]
        public void Should_apply_time_to_live()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var converter = new DefaultOutgoingMessagesToBrokeredMessagesConverter(settings);

            var ttl = TimeSpan.FromMinutes(1);
            var headers = new Dictionary<string, string>()
            {
                {Headers.TimeToBeReceived, ttl.ToString()}
            };

            var outgoingMessage = new OutgoingMessage("SomeId", headers, new byte[0]);
            var dispatchOptions = new DispatchOptions(new DirectToTargetDestination("MyQueue"), DispatchConsistency.Default, new List<DeliveryConstraint>());

            var brokeredMessage = converter.Convert(outgoingMessage, dispatchOptions);

            Assert.IsTrue(brokeredMessage.TimeToLive == ttl);
        }

        [Test]
        public void Should_apply_correlationid()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var converter = new DefaultOutgoingMessagesToBrokeredMessagesConverter(settings);

            var correlationId = "SomeId";
            var headers = new Dictionary<string, string>()
            {
                {Headers.CorrelationId, correlationId}
            };

            var outgoingMessage = new OutgoingMessage("SomeId", headers, new byte[0]);
            var dispatchOptions = new DispatchOptions(new DirectToTargetDestination("MyQueue"), DispatchConsistency.Default, new List<DeliveryConstraint>());

            var brokeredMessage = converter.Convert(outgoingMessage, dispatchOptions);

            Assert.IsTrue(brokeredMessage.CorrelationId == correlationId);
        }

        [Test]
        public void Should_set_replytoaddress()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var converter = new DefaultOutgoingMessagesToBrokeredMessagesConverter(settings);

            var replyto = "MyQueue";
            var headers = new Dictionary<string, string>()
            {
                {Headers.ReplyToAddress, replyto}
            };

            var outgoingMessage = new OutgoingMessage("SomeId", headers, new byte[0]);
            var dispatchOptions = new DispatchOptions(new DirectToTargetDestination("MyQueue"), DispatchConsistency.Default, new List<DeliveryConstraint>());

            var brokeredMessage = converter.Convert(outgoingMessage, dispatchOptions);

            Assert.IsTrue(brokeredMessage.ReplyTo == replyto);
        }
    }
}