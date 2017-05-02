namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Sending
{
    using AzureServiceBus;
    using Transport.AzureServiceBus;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_defaults_for_sending
    {
        [Test]
        public void SendVia_should_be_enabled()
        {
            // default settings
            var settings = DefaultConfigurationValues.Apply(new SettingsHolder());
            Assert.IsTrue(settings.Get<bool>(WellKnownConfigurationKeys.Connectivity.SendViaReceiveQueue));
        }
    }
}