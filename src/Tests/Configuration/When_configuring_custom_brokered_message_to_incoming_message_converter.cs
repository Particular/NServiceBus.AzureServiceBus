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
    }
}