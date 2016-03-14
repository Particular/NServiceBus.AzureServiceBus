namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using NServiceBus.AzureServiceBus;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_serialization
    {
        [Test]
        public void Should_be_able_to_set_the_brokered_message_body_type()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.BrokeredMessageBodyType(SupportedBrokeredMessageBodyTypes.Stream);

            Assert.AreEqual(SupportedBrokeredMessageBodyTypes.Stream, settings.Get<SupportedBrokeredMessageBodyTypes>(WellKnownConfigurationKeys.Serialization.BrokeredMessageBodyType));
        }
    }
}