namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Receiving
{
    using System;
    using Microsoft.ServiceBus.Messaging;
    using NUnit.Framework;
    using Settings;
    using Transport.AzureServiceBus;

    [TestFixture]
    public class When_converting_incoming_brokered_message
    {
        [Test]
        public void Should_return_empty_byte_array_for_bytearray_encoded_message()
        {
            var message = new BrokeredMessage((byte[])null);
            message.Properties[BrokeredMessageHeaders.TransportEncoding] = "wcf/byte-array";

            var settings = new SettingsHolder();
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, new NamespaceConfigurations());
            settings.Set(WellKnownConfigurationKeys.Serialization.BrokeredMessageBodyType, SupportedBrokeredMessageBodyTypes.ByteArray);

            var converter = new BrokeredMessagesToIncomingMessagesConverter(settings, new DefaultConnectionStringToNamespaceAliasMapper(settings));

            var converted = converter.Convert(message);
            Assert.AreEqual(Array.Empty<byte>(), converted.Body);
        }

        [Test]
        public void Should_return_empty_byte_array_for_octectstream_encoded_message()
        {
            var message = new BrokeredMessage(null);
            message.Properties[BrokeredMessageHeaders.TransportEncoding] = "application/octect-stream";

            var settings = new SettingsHolder();
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, new NamespaceConfigurations());
            settings.Set(WellKnownConfigurationKeys.Serialization.BrokeredMessageBodyType, SupportedBrokeredMessageBodyTypes.Stream);

            var converter = new BrokeredMessagesToIncomingMessagesConverter(settings, new DefaultConnectionStringToNamespaceAliasMapper(settings));

            var converted = converter.Convert(message);
            Assert.AreEqual(Array.Empty<byte>(), converted.Body);
        }
    }
}