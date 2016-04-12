namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Sending
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.AzureServiceBus.Topology.MetaModel;
    using NServiceBus.DelayedDelivery;
    using NServiceBus.DeliveryConstraints;
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

            var converter = new DefaultBatchedOperationsToBrokeredMessagesConverter(settings, new FakeMapper());

            var bytes = Encoding.UTF8.GetBytes("Whatever");

            var batchedOperation = new BatchedOperation
            {
                Message = new OutgoingMessage("SomeId", new Dictionary<string, string>(), bytes),
                DeliveryConstraints = new List<DeliveryConstraint>()
            };

            var brokeredMessage = converter.Convert(batchedOperation, new RoutingOptions());

            var body = Encoding.UTF8.GetString(brokeredMessage.GetBody<byte[]>());

            Assert.AreEqual(body, "Whatever");
        }

        [Test]
        public void Should_extract_body_as_stream_when_configured()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.BrokeredMessageBodyType(SupportedBrokeredMessageBodyTypes.Stream);

            var converter = new DefaultBatchedOperationsToBrokeredMessagesConverter(settings, new FakeMapper());

            var bytes = Encoding.UTF8.GetBytes("Whatever");

            var batchedOperation = new BatchedOperation
            {
                Message = new OutgoingMessage("SomeId", new Dictionary<string, string>(), bytes),
                DeliveryConstraints = new List<DeliveryConstraint>()
            };

            var brokeredMessage = converter.Convert(batchedOperation, new RoutingOptions());

            var sr = new StreamReader(brokeredMessage.GetBody<Stream>());
            var body = sr.ReadToEnd();

            Assert.AreEqual("Whatever", body);
        }

        [Test]
        public void Should_copy_the_message_id()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var converter = new DefaultBatchedOperationsToBrokeredMessagesConverter(settings, new FakeMapper());

            var batchedOperation = new BatchedOperation
            {
                Message = new OutgoingMessage("SomeId", new Dictionary<string, string>(), new byte[0]),
                DeliveryConstraints = new List<DeliveryConstraint>()
            };

            var brokeredMessage = converter.Convert(batchedOperation, new RoutingOptions());

            Assert.IsTrue(brokeredMessage.MessageId == "SomeId");
        }

        [Test]
        public void Should_copy_the_headers()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var converter = new DefaultBatchedOperationsToBrokeredMessagesConverter(settings, new FakeMapper());

            var headers = new Dictionary<string, string>
            {
                {"MyHeader", "MyValue"}
            };

            var batchedOperation = new BatchedOperation
            {
                Message = new OutgoingMessage("SomeId", headers, new byte[0]),
                DeliveryConstraints = new List<DeliveryConstraint>()
            };

            var brokeredMessage = converter.Convert(batchedOperation, new RoutingOptions());

            Assert.IsTrue(brokeredMessage.Properties.ContainsKey("MyHeader"));
            Assert.AreEqual("MyValue", brokeredMessage.Properties["MyHeader"]);
        }

        [Test]
        public void Should_apply_delayed_delivery()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var converter = new DefaultBatchedOperationsToBrokeredMessagesConverter(settings, new FakeMapper());

            var now = DateTime.UtcNow;
            Time.UtcNow = () => now;

            var delay = new DelayDeliveryWith(TimeSpan.FromDays(1));

            var batchedOperation = new BatchedOperation
            {
                Message = new OutgoingMessage("SomeId", new Dictionary<string, string>(), new byte[0]),
                DeliveryConstraints = new List<DeliveryConstraint> { delay }
            };

            var brokeredMessage = converter.Convert(batchedOperation, new RoutingOptions());

            Assert.IsTrue(brokeredMessage.ScheduledEnqueueTimeUtc == now.AddDays(1));

            Time.UtcNow = () => DateTime.UtcNow;
        }

        [Test]
        public void Should_apply_delivery_at_specific_date()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var converter = new DefaultBatchedOperationsToBrokeredMessagesConverter(settings, new FakeMapper());

            var now = DateTime.UtcNow;
            Time.UtcNow = () => now;
            var delay = new DoNotDeliverBefore(now.AddDays(2));

            var batchedOperation = new BatchedOperation
            {
                Message = new OutgoingMessage("SomeId", new Dictionary<string, string>(), new byte[0]),
                DeliveryConstraints = new List<DeliveryConstraint> { delay }
            };

            var brokeredMessage = converter.Convert(batchedOperation, new RoutingOptions());

            Assert.IsTrue(brokeredMessage.ScheduledEnqueueTimeUtc == now.AddDays(2));

            Time.UtcNow = () => DateTime.UtcNow;
        }

        [Test]
        public void Should_apply_time_to_live()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var converter = new DefaultBatchedOperationsToBrokeredMessagesConverter(settings, new FakeMapper());

            var ttl = TimeSpan.FromMinutes(1);

            var headers = new Dictionary<string, string>
            {
                {Headers.TimeToBeReceived, ttl.ToString()}
            };

            var batchedOperation = new BatchedOperation
            {
                Message = new OutgoingMessage("SomeId", headers, new byte[0]),
                DeliveryConstraints = new List<DeliveryConstraint>()
            };

            var brokeredMessage = converter.Convert(batchedOperation, new RoutingOptions());

            Assert.IsTrue(brokeredMessage.TimeToLive == ttl);
        }

        [Test]
        public void Should_apply_correlationid()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var converter = new DefaultBatchedOperationsToBrokeredMessagesConverter(settings, new FakeMapper());

            var correlationId = "SomeId";
            var headers = new Dictionary<string, string>
            {
                {Headers.CorrelationId, correlationId}
            };

            var batchedOperation = new BatchedOperation
            {
                Message = new OutgoingMessage("SomeId", headers, new byte[0]),
                DeliveryConstraints = new List<DeliveryConstraint>()
            };

            var brokeredMessage = converter.Convert(batchedOperation, new RoutingOptions());

            Assert.IsTrue(brokeredMessage.CorrelationId == correlationId);
        }

        [Test]
        public void Should_set_replytoaddress()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var converter = new DefaultBatchedOperationsToBrokeredMessagesConverter(settings, new FakeMapper("MyQueue", "MappedMyQueue"));

            var headers = new Dictionary<string, string>()
            {
                {Headers.ReplyToAddress, "MyQueue"}
            };

            var batchedOperation = new BatchedOperation
            {
                Message = new OutgoingMessage("SomeId", headers, new byte[0]),
                DeliveryConstraints = new List<DeliveryConstraint>()
            };

            var brokeredMessage = converter.Convert(batchedOperation, new RoutingOptions());

            Assert.IsTrue(brokeredMessage.ReplyTo == "MappedMyQueue");
        }

        [Test]
        public void Should_set_ViaPartitionKey_if_partition_key_is_available_and_sending_via_option_is_enabled()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var converter = new DefaultBatchedOperationsToBrokeredMessagesConverter(settings, new FakeMapper());

            var routingOptions = new RoutingOptions
            {
                SendVia = true,
                ViaPartitionKey = "partitionkey"
            };

            var batchedOperation = new BatchedOperation
            {
                Message = new OutgoingMessage("SomeId", new Dictionary<string, string>(), new byte[0]),
                DeliveryConstraints = new List<DeliveryConstraint>()
            };

            var brokeredMessage = converter.Convert(batchedOperation, routingOptions);

            Assert.IsTrue(brokeredMessage.ViaPartitionKey == "partitionkey");
        }

        [TestCase(SupportedBrokeredMessageBodyTypes.Stream, "application/octect-stream")]
        [TestCase(SupportedBrokeredMessageBodyTypes.ByteArray, "wcf/byte-array")]
        public void Should_set_transport_encoding_header(SupportedBrokeredMessageBodyTypes bodyType, string expectedHeaderValue)
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.BrokeredMessageBodyType(bodyType);

            var converter = new DefaultBatchedOperationsToBrokeredMessagesConverter(settings, new FakeMapper());

            var bytes = Encoding.UTF8.GetBytes("Whatever");

            var batchedOperation = new BatchedOperation
            {
                Message = new OutgoingMessage("SomeId", new Dictionary<string, string>(), bytes),
                DeliveryConstraints = new List<DeliveryConstraint>()
            };

            var brokeredMessage = converter.Convert(batchedOperation, new RoutingOptions());

            Assert.That(brokeredMessage.Properties[BrokeredMessageHeaders.TransportEncoding], Is.EqualTo(expectedHeaderValue));
        }

        [Test]
        public void Should_inject_estimated_message_size_into_headers()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var converter = new DefaultBatchedOperationsToBrokeredMessagesConverter(settings, new FakeMapper());

            var body = Encoding.UTF8.GetBytes("Whatever");
            var headers = new Dictionary<string, string> { { "header", "value" } };
            var batchedOperation = new BatchedOperation
            {
                Message = new OutgoingMessage("SomeId", headers, body),
                DeliveryConstraints = new List<DeliveryConstraint>()
            };

            var brokeredMessage = converter.Convert(batchedOperation, new RoutingOptions());

            Assert.That(brokeredMessage.Properties[BrokeredMessageHeaders.EstimatedMessageSize], Is.GreaterThan(0)); 
        }

        private class FakeMapper : ICanMapNamespaceNameToConnectionString
        {
            private readonly string input;
            private readonly string output;

            public FakeMapper()
                : this ("input", "output")
            {
                
            }

            public FakeMapper(string input, string output)
            {
                this.input = input;
                this.output = output;
            }

            public EntityAddress Map(EntityAddress value)
            {
                if (input != value)
                    throw new InvalidOperationException();

                return output;
            }
        }
    }
}