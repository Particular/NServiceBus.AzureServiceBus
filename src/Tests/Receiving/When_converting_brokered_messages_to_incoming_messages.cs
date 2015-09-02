namespace NServiceBus.AzureServiceBus.Tests
{
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_converting_brokered_messages_to_incoming_messages
    {
        [Test]
        public async Task Should_copy_the_message_id()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings);

            var brokeredMessage = new BrokeredMessage
            {
                MessageId = "someid"
            };

            var incomingMessage = converter.Convert(brokeredMessage);

            Assert.IsTrue(incomingMessage.MessageId == "someid");
        }

        [Test]
        public async Task Should_copy_properties_into_the_headers()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings);

            var brokeredMessage = new BrokeredMessage();
            brokeredMessage.Properties.Add("my-test-prop", "myvalue");

            var incomingMessage = converter.Convert(brokeredMessage);

            Assert.IsTrue(incomingMessage.Headers.Any(h => h.Key == "my-test-prop" && h.Value == "myvalue"));
        }

        [Test]
        public async Task Should_extract_body_as_byte_array_by_default()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings);

            var bytes = Encoding.UTF8.GetBytes("Whatever");

            var brokeredMessage = new BrokeredMessage(bytes);

            var incomingMessage = converter.Convert(brokeredMessage);

            var sr = new StreamReader(incomingMessage.BodyStream);
            var body = sr.ReadToEnd();

            Assert.AreEqual(body, "Whatever");
        }

        [Test]
        public async Task Should_extract_body_as_stream_when_configured()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Serialization().BrokeredMessageBodyType(SupportedBrokeredMessageBodyTypes.Stream);

            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings);

            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write("Whatever");
            writer.Flush();
            stream.Position = 0;

            var brokeredMessage = new BrokeredMessage(stream);

            var incomingMessage = converter.Convert(brokeredMessage);

            var sr = new StreamReader(incomingMessage.BodyStream);
            var body = sr.ReadToEnd();

            Assert.AreEqual("Whatever", body);
        }
    }
}