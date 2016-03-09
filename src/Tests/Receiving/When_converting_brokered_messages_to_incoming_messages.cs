namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Receiving
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.AzureServiceBus.Topology.MetaModel;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_converting_brokered_messages_to_incoming_messages
    {
        [Test]
        public void Should_copy_the_message_id()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings, new FakeMapper("", ""));

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
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            
            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings, new FakeMapper("", ""));

            var brokeredMessage = new BrokeredMessage();
            brokeredMessage.Properties.Add("my-test-prop", "myvalue");

            var incomingMessage = converter.Convert(brokeredMessage);

            Assert.IsTrue(incomingMessage.Headers.Any(h => h.Key == "my-test-prop" && h.Value == "myvalue"));
        }

        [Test]
        public void Should_complete_replyto_address_if_not_present_in_headers()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            
            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings, new FakeMapper("MyQueue", "MappedMyQueue"));

            var brokeredMessage = new BrokeredMessage(new byte[] { })
            {
                ReplyTo = "MyQueue"
            };

            var incomingMessage = converter.Convert(brokeredMessage);

            Assert.IsTrue(incomingMessage.Headers.ContainsKey(Headers.ReplyToAddress));
            Assert.AreEqual("MappedMyQueue", incomingMessage.Headers[Headers.ReplyToAddress]);
        }

        [Test]
        public void Should_not_complete_replyto_address_if_already_present_in_headers()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings, new FakeMapper("OtherQueue", "MappedOtherQueue"));

            var brokeredMessage = new BrokeredMessage(new byte[] { })
            {
                ReplyTo = "MyQueue"
            };
            brokeredMessage.Properties.Add(Headers.ReplyToAddress, "OtherQueue");

            var incomingMessage = converter.Convert(brokeredMessage);

            Assert.IsTrue(incomingMessage.Headers.ContainsKey(Headers.ReplyToAddress));
            Assert.AreEqual("MappedOtherQueue", incomingMessage.Headers[Headers.ReplyToAddress]);
        }

        [Test]
        public void Should_complete_correlationid_if_not_present_in_headers()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings, new FakeMapper("",""));

            var brokeredMessage = new BrokeredMessage(new byte[] {})
            {
                CorrelationId = "SomeId"
            };


            var incomingMessage = converter.Convert(brokeredMessage);

            Assert.IsTrue(incomingMessage.Headers.ContainsKey(Headers.CorrelationId));
            Assert.AreEqual("SomeId", incomingMessage.Headers[Headers.CorrelationId]);
        }

        [Test]
        public void Should_complete_timetobereceived_if_not_present_in_headers()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings, new FakeMapper("", ""));

            var timespan = TimeSpan.FromHours(1);
            var brokeredMessage = new BrokeredMessage(new byte[] { })
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
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings, new FakeMapper("", ""));

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
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.Serialization().BrokeredMessageBodyType(SupportedBrokeredMessageBodyTypes.Stream);

            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings, new FakeMapper("", ""));

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
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings, new FakeMapper("", ""));

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
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings, new FakeMapper("", ""));

            var brokeredMessage = new BrokeredMessage("non-default-type");

            Assert.Throws<UnsupportedBrokeredMessageBodyTypeException>(() => converter.Convert(brokeredMessage));
        }

        [Test]
        public void Should_throw_for_a_message_with_unknown_transport_encoding_header_supplied()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings, new FakeMapper("", ""));

            var brokeredMessage = new BrokeredMessage("non-default-type");
            brokeredMessage.Properties[BrokeredMessageHeaders.TransportEncoding] = "unknown";

            Assert.Throws<UnsupportedBrokeredMessageBodyTypeException>(() => converter.Convert(brokeredMessage));
        }

        [Test]
        public void Should_not_propagate_transport_encoding_header_from_brokered_message()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var bytes = Encoding.UTF8.GetBytes("Whatever");
            var brokeredMessage = new BrokeredMessage(bytes);
            brokeredMessage.Properties[BrokeredMessageHeaders.TransportEncoding] = "wcf/byte-array";
            
            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings, new FakeMapper("", ""));

            var incomingMessageDetails = converter.Convert(brokeredMessage);

            CollectionAssert.DoesNotContain(incomingMessageDetails.Headers, BrokeredMessageHeaders.TransportEncoding, $"Headers should not contain `{BrokeredMessageHeaders.TransportEncoding}`, but it was found.");
        }

        private class FakeMapper : ICanMapConnectionStringToNamespaceName
        {
            private readonly string _input;
            private readonly string _output;

            public FakeMapper(string input, string output)
            {
                _input = input;
                _output = output;
            }

            public EntityAddress Map(EntityAddress value)
            {
                if (_input != value)
                    throw new InvalidOperationException();

                return _output;
            }
        }
    }
}