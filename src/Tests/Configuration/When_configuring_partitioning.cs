namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_partitioning
    {
        [Test]
        public void Should_be_able_to_set_the_partitioning_strategy()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var topicSettings = extensions.Topology().Addressing().Partitioning().Strategy<MyPartitioningStrategy>();

            Assert.AreEqual(typeof(MyPartitioningStrategy), topicSettings.GetSettings().Get<Type>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Strategy));
        }

        class MyPartitioningStrategy : IPartitioningStrategy
        {
        }
    }
}