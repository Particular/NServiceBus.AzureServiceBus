namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using System;
    using AzureServiceBus;
    using NUnit.Framework;
    using Settings;
    using Transport.AzureServiceBus;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_custom_outgoing_message_to_brokered_message_converter
    {
        [Test]
        public void Default_value_should_be_configured()
        {
            var settings = new SettingsHolder();
            new DefaultConfigurationValues().Apply(settings);

            Assert.AreEqual(typeof(DefaultBatchedOperationsToBrokeredMessagesConverter), settings.Get<Type>(WellKnownConfigurationKeys.BrokeredMessageConventions.FromOutgoingMessageConverter));
        }
    }
}