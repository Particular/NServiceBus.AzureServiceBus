namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using System.Configuration;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.ServiceBus.Messaging;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_converting_brokered_messages_to_incoming_messages
    {
        [Test]
        public void Should_copy_the_message_id()
        {
            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter();

            var brokeredMessage = new BrokeredMessage
            {
                MessageId = "someid"
            };

            var incomingMessage = converter.Convert(brokeredMessage);

            Assert.IsTrue(incomingMessage.MessageId == "someid");
        }

        [Test]
        public void Should_copy_properties_into_the_headers()
        {
            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter();

            var brokeredMessage = new BrokeredMessage();
            brokeredMessage.Properties.Add("my-test-prop", "myvalue");

            var incomingMessage = converter.Convert(brokeredMessage);

            Assert.IsTrue(incomingMessage.Headers.Any(h => h.Key == "my-test-prop" && h.Value == "myvalue"));
        }

        public void Should_complete_replyto_address_if_not_present_in_headers()
        {
            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter();

            var brokeredMessage = new BrokeredMessage("")
            {
                ReplyTo = "MyQueue"
            };


            var incomingMessage = converter.Convert(brokeredMessage);

            Assert.IsTrue(incomingMessage.Headers.ContainsKey(Headers.ReplyToAddress));
            Assert.AreEqual("MyQueue", incomingMessage.Headers[Headers.ReplyToAddress]);
        }

        public void Should_complete_correlationid_if_not_present_in_headers()
        {
            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter();

            var brokeredMessage = new BrokeredMessage("")
            {
                CorrelationId = "SomeId"
            };


            var incomingMessage = converter.Convert(brokeredMessage);

            Assert.IsTrue(incomingMessage.Headers.ContainsKey(Headers.CorrelationId));
            Assert.AreEqual("SomeId", incomingMessage.Headers[Headers.CorrelationId]);
        }

        public void Should_complete_timetobereceived_if_not_present_in_headers()
        {
            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter();

            var timespan = TimeSpan.FromHours(1);
            var brokeredMessage = new BrokeredMessage("")
            {
                TimeToLive = timespan
            };


            var incomingMessage = converter.Convert(brokeredMessage);

            Assert.IsTrue(incomingMessage.Headers.ContainsKey(Headers.TimeToBeReceived));
            Assert.AreEqual(timespan.ToString(), incomingMessage.Headers[Headers.TimeToBeReceived]);
        }

        [Test]
        public void Should_extract_body_as_byte_array_by_default()
        {
            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter();

            var bytes = Encoding.UTF8.GetBytes("Whatever");
            var brokeredMessage = new BrokeredMessage(bytes);

            var incomingMessage = converter.Convert(brokeredMessage);

            var sr = new StreamReader(incomingMessage.BodyStream);
            var body = sr.ReadToEnd();

            Assert.AreEqual(body, "Whatever");
        }

        [Test]
        public void Should_extract_body_as_stream_when_configured()
        {
            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter();

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write("Whatever");
            writer.Flush();
            stream.Position = 0;

            var brokeredMessage = new BrokeredMessage(stream);
            brokeredMessage.Properties[BrokeredMessageHeaders.TransportEncoding] = "application/octect-stream";

            var incomingMessage = converter.Convert(brokeredMessage);

            var sr = new StreamReader(incomingMessage.BodyStream);
            var body = sr.ReadToEnd();

            Assert.AreEqual("Whatever", body);
        }

        [Test]
        public void Should_extract_body_as_byte_array_when_configured()
        {
            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter();
            var bytes = Encoding.UTF8.GetBytes("Whatever");
            var brokeredMessage = new BrokeredMessage(bytes);
            brokeredMessage.Properties[BrokeredMessageHeaders.TransportEncoding] = "wcf/byte-array";

            var incomingMessage = converter.Convert(brokeredMessage);

            var sr = new StreamReader(incomingMessage.BodyStream);
            var body = sr.ReadToEnd();

            Assert.AreEqual("Whatever", body);
        }

        [Test]
        public void Should_throw_for_a_message_without_transport_encoding_header_supplied_and_actual_body_type_other_than_default()
        {
            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter();

            var brokeredMessage = new BrokeredMessage("non-default-type");

            Assert.Throws<ConfigurationErrorsException>(() => converter.Convert(brokeredMessage));
        }

        [Test]
        public void Should_throw_for_a_message_with_unknown_transport_encoding_header_supplied()
        {
            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter();

            var brokeredMessage = new BrokeredMessage("non-default-type");
            brokeredMessage.Properties[BrokeredMessageHeaders.TransportEncoding] = "unknown";

            Assert.Throws<ConfigurationErrorsException>(() => converter.Convert(brokeredMessage));
        }
    }
}