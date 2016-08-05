namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using System;
    using AzureServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NUnit.Framework;
    using Settings;
    using Transport.AzureServiceBus;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_custom_brokered_message_to_incoming_message_converter
    {
        [Test]
        public void Default_value_should_be_configured()
        {
            var settings = new SettingsHolder();
            new DefaultConfigurationValues().Apply(settings);

            Assert.AreEqual(typeof(DefaultBrokeredMessagesToIncomingMessagesConverter), settings.Get<Type>(WellKnownConfigurationKeys.BrokeredMessageConventions.ToIncomingMessageConverter));
        }

        [Test]
        public void Should_be_able_to_set_custom_converter()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.UseBrokeredMessageToIncomingMessageConverter<ConvertBrokeredMessagesToIncomingMessages>();

            Assert.AreEqual(typeof(ConvertBrokeredMessagesToIncomingMessages), settings.Get<Type>(WellKnownConfigurationKeys.BrokeredMessageConventions.ToIncomingMessageConverter));
        }

        class ConvertBrokeredMessagesToIncomingMessages : IConvertBrokeredMessagesToIncomingMessages
        {
            public IncomingMessageDetails Convert(BrokeredMessage brokeredMessage)
            {
                throw new NotImplementedException();
            }
        }
    }
}