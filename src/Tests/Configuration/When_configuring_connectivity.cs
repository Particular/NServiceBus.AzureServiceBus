namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using Transport.AzureServiceBus;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_connectivity
    {
        [Test]
        public void Should_be_able_to_set_number_of_clients_per_entity()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.NumberOfClientsPerEntity(4);

            Assert.AreEqual(4, settings.Get<int>(WellKnownConfigurationKeys.Connectivity.NumberOfClientsPerEntity));
        }

        public void Should_be_able_to_set_whether_send_via_receive_queue_should_be_used()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.SendViaReceiveQueue(false);

            Assert.AreEqual(false, settings.Get<bool>(WellKnownConfigurationKeys.Connectivity.SendViaReceiveQueue));
        }
    }
}