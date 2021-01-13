namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Sending
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using AzureServiceBus;
    using Transport.AzureServiceBus;
    using DelayedDelivery;
    using DeliveryConstraints;
    using Transport;
    using NUnit.Framework;
    using Performance.TimeToBeReceived;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_converting_outgoing_messages_to_brokered_messages
    {
        [Test]
        public void Should_inject_body_as_byte_array_by_default()
        {
            // default settings
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());

            var converter = new BatchedOperationsToBrokeredMessagesConverter(settings);

            var bytes = Encoding.UTF8.GetBytes("Whatever");

            var batchedOperation = new BatchedOperationInternal
            {
                Message = new OutgoingMessage("SomeId", new Dictionary<string, string>(), bytes),
                DeliveryConstraints = new List<DeliveryConstraint>()
            };

            var brokeredMessage = converter.Convert(batchedOperation, new RoutingOptionsInternal());

            var body = Encoding.UTF8.GetString(brokeredMessage.GetBody<byte[]>());

            Assert.AreEqual(body, "Whatever");
        }

        [Test]
        public void Should_extract_body_as_stream_when_configured()
        {
            // default settings
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.BrokeredMessageBodyType(SupportedBrokeredMessageBodyTypes.Stream);

            var converter = new BatchedOperationsToBrokeredMessagesConverter(settings);

            var bytes = Encoding.UTF8.GetBytes("Whatever");

            var batchedOperation = new BatchedOperationInternal
            {
                Message = new OutgoingMessage("SomeId", new Dictionary<string, string>(), bytes),
                DeliveryConstraints = new List<DeliveryConstraint>()
            };

            var brokeredMessage = converter.Convert(batchedOperation, new RoutingOptionsInternal());

            var sr = new StreamReader(brokeredMessage.GetBody<Stream>());
            var body = sr.ReadToEnd();

            Assert.AreEqual("Whatever", body);
        }

        [Test]
        public void Should_NOT_copy_the_message_id()
        {
            // default settings
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var converter = new BatchedOperationsToBrokeredMessagesConverter(settings);

            var batchedOperation = new BatchedOperationInternal
            {
                Message = new OutgoingMessage("SomeId", new Dictionary<string, string>(), new byte[0]),
                DeliveryConstraints = new List<DeliveryConstraint>()
            };

            var brokeredMessage = converter.Convert(batchedOperation, new RoutingOptionsInternal());

            Assert.That(brokeredMessage.MessageId, Is.Not.EqualTo("SomeId"));
        }

        [Test]
        public void Should_copy_the_headers()
        {
            // default settings
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());

            var converter = new BatchedOperationsToBrokeredMessagesConverter(settings);

            var headers = new Dictionary<string, string>
            {
                {"MyHeader", "MyValue"}
            };

            var batchedOperation = new BatchedOperationInternal
            {
                Message = new OutgoingMessage("SomeId", headers, new byte[0]),
                DeliveryConstraints = new List<DeliveryConstraint>()
            };

            var brokeredMessage = converter.Convert(batchedOperation, new RoutingOptionsInternal());

            Assert.IsTrue(brokeredMessage.Properties.ContainsKey("MyHeader"));
            Assert.AreEqual("MyValue", brokeredMessage.Properties["MyHeader"]);
        }

        [Test]
        public void Should_apply_delayed_delivery()
        {
            // default settings
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());

            var converter = new BatchedOperationsToBrokeredMessagesConverter(settings);

            var now = DateTime.UtcNow;
            Time.UtcNow = () => now;

            var delay = new DelayDeliveryWith(TimeSpan.FromDays(1));

            var batchedOperation = new BatchedOperationInternal
            {
                Message = new OutgoingMessage("SomeId", new Dictionary<string, string>(), new byte[0]),
                DeliveryConstraints = new List<DeliveryConstraint> { delay }
            };

            var brokeredMessage = converter.Convert(batchedOperation, new RoutingOptionsInternal());

            Assert.IsTrue(brokeredMessage.ScheduledEnqueueTimeUtc == now.AddDays(1));

            Time.UtcNow = () => DateTime.UtcNow;
        }

        [Test]
        public void Should_apply_delivery_at_specific_date()
        {
            // default settings
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());

            var converter = new BatchedOperationsToBrokeredMessagesConverter(settings);

            var now = DateTime.UtcNow;
            Time.UtcNow = () => now;
            var delay = new DoNotDeliverBefore(now.AddDays(2));

            var batchedOperation = new BatchedOperationInternal
            {
                Message = new OutgoingMessage("SomeId", new Dictionary<string, string>(), new byte[0]),
                DeliveryConstraints = new List<DeliveryConstraint> { delay }
            };

            var brokeredMessage = converter.Convert(batchedOperation, new RoutingOptionsInternal());

            Assert.IsTrue(brokeredMessage.ScheduledEnqueueTimeUtc == now.AddDays(2));

            Time.UtcNow = () => DateTime.UtcNow;
        }

        [Test]
        public void Should_apply_time_to_live()
        {
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());

            var converter = new BatchedOperationsToBrokeredMessagesConverter(settings);

            var ttl = TimeSpan.FromMinutes(1);

            var headers = new Dictionary<string, string>
            {
                {Headers.TimeToBeReceived, ttl.ToString()}
            };

            var batchedOperation = new BatchedOperationInternal
            {
                Message = new OutgoingMessage("SomeId", headers, new byte[0]),
                DeliveryConstraints = new List<DeliveryConstraint> { new DiscardIfNotReceivedBefore(ttl) }
            };

            var brokeredMessage = converter.Convert(batchedOperation, new RoutingOptionsInternal());

            Assert.IsTrue(brokeredMessage.TimeToLive == ttl);
        }

        [Test]
        public void Should_apply_correlationid()
        {
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());

            var converter = new BatchedOperationsToBrokeredMessagesConverter(settings);

            var correlationId = "SomeId";
            var headers = new Dictionary<string, string>
            {
                {Headers.CorrelationId, correlationId}
            };

            var batchedOperation = new BatchedOperationInternal
            {
                Message = new OutgoingMessage("SomeId", headers, new byte[0]),
                DeliveryConstraints = new List<DeliveryConstraint>()
            };

            var brokeredMessage = converter.Convert(batchedOperation, new RoutingOptionsInternal());

            Assert.IsTrue(brokeredMessage.CorrelationId == correlationId);
        }

        [Test]
        public void Should_set_replyto_address()
        {
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());

            var converter = new BatchedOperationsToBrokeredMessagesConverter(settings);

            var headers = new Dictionary<string, string>
            {
                {Headers.ReplyToAddress, "MyQueue"}
            };

            var batchedOperation = new BatchedOperationInternal
            {
                Message = new OutgoingMessage("SomeId", headers, new byte[0]),
                DeliveryConstraints = new List<DeliveryConstraint>()
            };

            var brokeredMessage = converter.Convert(batchedOperation, new RoutingOptionsInternal());

            Assert.IsTrue(brokeredMessage.ReplyTo == "MyQueue"); // the mapper should be ignored, need to respect user's setting
        }

        [TestCase(true, "MyQueue@alias")]
        [TestCase(false, "MyQueue@Endpoint=sb://name-x.servicebus.windows.net;SharedAccessKeyName=keyname;SharedAccessKey=key")]
        public void Should_set_replyto_address_with_respect_to_secured_connection_strings_setting(bool shouldSecureConnectionString, string expectedReplyToAddress)
        {
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());

            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.UseNamespaceAliasesInsteadOfConnectionStrings, shouldSecureConnectionString);
            var namespaces = new NamespaceConfigurations(new List<NamespaceInfo>
            {
                new NamespaceInfo("alias", "Endpoint=sb://name-x.servicebus.windows.net;SharedAccessKeyName=keyname;SharedAccessKey=key")
            });
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, namespaces);

            var headers = new Dictionary<string, string>
            {
                {Headers.ReplyToAddress, "MyQueue"}
            };

            var batchedOperation = new BatchedOperationInternal
            {
                Message = new OutgoingMessage("SomeId", headers, new byte[0]),
                DeliveryConstraints = new List<DeliveryConstraint>()
            };

            var converter = new BatchedOperationsToBrokeredMessagesConverter(settings);
            var brokeredMessage = converter.Convert(batchedOperation, new RoutingOptionsInternal());

            Assert.That(brokeredMessage.ReplyTo, Is.EqualTo(expectedReplyToAddress));
        }

        [TestCase(true, "MyQueue@alias2")]
        [TestCase(false, "MyQueue@Endpoint=sb://name-y.servicebus.windows.net;SharedAccessKeyName=keyname;SharedAccessKey=key")]
        public void Should_set_replyto_address_to_destination_if_multiple_available_with_respect_to_secured_connection_strings_setting(bool shouldSecureConnectionString, string expectedReplyToAddress)
        {
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());

            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.UseNamespaceAliasesInsteadOfConnectionStrings, shouldSecureConnectionString);
            var namespaces = new NamespaceConfigurations(new List<NamespaceInfo>
            {
                new NamespaceInfo("alias1", "Endpoint=sb://name-x.servicebus.windows.net;SharedAccessKeyName=keyname;SharedAccessKey=key"),
                new NamespaceInfo("alias2", "Endpoint=sb://name-y.servicebus.windows.net;SharedAccessKeyName=keyname;SharedAccessKey=key")
            });
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, namespaces);

            var headers = new Dictionary<string, string>
            {
                {Headers.ReplyToAddress, "MyQueue"}
            };

            var batchedOperation = new BatchedOperationInternal
            {
                Message = new OutgoingMessage("SomeId", headers, new byte[0]),
                DeliveryConstraints = new List<DeliveryConstraint>()
            };

            var converter = new BatchedOperationsToBrokeredMessagesConverter(settings);
            var brokeredMessage = converter.Convert(batchedOperation, new RoutingOptionsInternal
            {
                DestinationNamespace = new RuntimeNamespaceInfo("alias2", "Endpoint=sb://name-y.servicebus.windows.net;SharedAccessKeyName=keyname;SharedAccessKey=key")
            });

            Assert.That(brokeredMessage.ReplyTo, Is.EqualTo(expectedReplyToAddress));
        }

        [Test]
        public void Should_set_ViaPartitionKey_if_partition_key_is_available_and_sending_via_option_is_enabled()
        {
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var converter = new BatchedOperationsToBrokeredMessagesConverter(settings);

            var routingOptions = new RoutingOptionsInternal
            {
                SendVia = true,
                ViaPartitionKey = "partitionkey"
            };

            var batchedOperation = new BatchedOperationInternal
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
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.BrokeredMessageBodyType(bodyType);

            var converter = new BatchedOperationsToBrokeredMessagesConverter(settings);

            var bytes = Encoding.UTF8.GetBytes("Whatever");

            var batchedOperation = new BatchedOperationInternal
            {
                Message = new OutgoingMessage("SomeId", new Dictionary<string, string>(), bytes),
                DeliveryConstraints = new List<DeliveryConstraint>()
            };

            var brokeredMessage = converter.Convert(batchedOperation, new RoutingOptionsInternal());

            Assert.That(brokeredMessage.Properties[BrokeredMessageHeaders.TransportEncoding], Is.EqualTo(expectedHeaderValue));
        }

        [Test]
        public void Should_inject_estimated_message_size_into_headers()
        {
            // default settings
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());

            var converter = new BatchedOperationsToBrokeredMessagesConverter(settings);

            var body = Encoding.UTF8.GetBytes("Whatever");
            var headers = new Dictionary<string, string> { { "header", "value" } };
            var batchedOperation = new BatchedOperationInternal
            {
                Message = new OutgoingMessage("SomeId", headers, body),
                DeliveryConstraints = new List<DeliveryConstraint>()
            };

            var brokeredMessage = converter.Convert(batchedOperation, new RoutingOptionsInternal());

            Assert.That(brokeredMessage.Properties[BrokeredMessageHeaders.EstimatedMessageSize], Is.GreaterThan(0));
        }
    }
}