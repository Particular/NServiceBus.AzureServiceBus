namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Receiving
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using AzureServiceBus;
    using Transport.AzureServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NUnit.Framework;
    using Settings;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_converting_brokered_messages_to_incoming_messages
    {
        [Test]
        public void Should_copy_the_message_id()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings, new FakeMapper("MyQueue", "MyQueue"));

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

            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings, new FakeMapper("MyQueue", "MyQueue"));

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

            var brokeredMessage = new BrokeredMessage(new byte[]
            {
            })
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

            var brokeredMessage = new BrokeredMessage(new byte[]
            {
            })
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

            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings, new FakeMapper("MyQueue", "MyQueue"));

            var brokeredMessage = new BrokeredMessage(new byte[]
            {
            })
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

            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings, new FakeMapper("MyQueue", "MyQueue"));

            var timespan = TimeSpan.FromHours(1);
            var brokeredMessage = new BrokeredMessage(new byte[]
            {
            })
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
            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings, new FakeMapper("MyQueue", "MyQueue"));

            var bytes = Encoding.UTF8.GetBytes("Whatever");
            var brokeredMessage = new BrokeredMessage(bytes);

            var incomingMessage = converter.Convert(brokeredMessage);

            var body = Encoding.UTF8.GetString(incomingMessage.Body);

            Assert.AreEqual(body, "Whatever");
        }

        [Test]
        public void Should_extract_body_as_stream_when_configured()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.BrokeredMessageBodyType(SupportedBrokeredMessageBodyTypes.Stream);

            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings, new FakeMapper("MyQueue", "MyQueue"));

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write("Whatever");
            writer.Flush();
            stream.Position = 0;

            var brokeredMessage = new BrokeredMessage(stream);
            brokeredMessage.Properties[BrokeredMessageHeaders.TransportEncoding] = "application/octect-stream";

            var incomingMessage = converter.Convert(brokeredMessage);

            var body = Encoding.UTF8.GetString(incomingMessage.Body);

            Assert.AreEqual("Whatever", body);
        }

        [Test]
        public void Should_extract_body_as_byte_array_when_configured()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings, new FakeMapper("MyQueue", "MyQueue"));

            var bytes = Encoding.UTF8.GetBytes("Whatever");
            var brokeredMessage = new BrokeredMessage(bytes);
            brokeredMessage.Properties[BrokeredMessageHeaders.TransportEncoding] = "wcf/byte-array";

            var incomingMessage = converter.Convert(brokeredMessage);

            var body = Encoding.UTF8.GetString(incomingMessage.Body);

            Assert.AreEqual("Whatever", body);
        }

        [Test]
        public void Should_throw_for_a_message_without_transport_encoding_header_supplied_and_actual_body_type_other_than_default()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings, new FakeMapper("MyQueue", "MyQueue"));

            var brokeredMessage = new BrokeredMessage("non-default-type");

            Assert.Throws<UnsupportedBrokeredMessageBodyTypeException>(() => converter.Convert(brokeredMessage));
        }

        [Test]
        public void Should_throw_for_a_message_with_unknown_transport_encoding_header_supplied()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings, new FakeMapper("MyQueue", "MyQueue"));

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

            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings, new FakeMapper("MyQueue", "MyQueue"));

            var incomingMessageDetails = converter.Convert(brokeredMessage);

            CollectionAssert.DoesNotContain(incomingMessageDetails.Headers, BrokeredMessageHeaders.TransportEncoding, $"Headers should not contain `{BrokeredMessageHeaders.TransportEncoding}`, but it was found.");
        }

        class FakeMapper : ICanMapConnectionStringToNamespaceAlias
        {
            public FakeMapper(string input, string output)
            {
                this.input = new EntityAddress(input);
                this.output = new EntityAddress(output);
            }

            public EntityAddress Map(EntityAddress value)
            {
                if (input != value)
                {
                    throw new InvalidOperationException();
                }

                return output;
            }

            EntityAddress input;
            EntityAddress output;
        }
    }
}