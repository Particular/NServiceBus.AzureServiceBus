namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using NServiceBus.AzureServiceBus.Topology.MetaModel;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_use_logical_namespace_name
    {
        private SettingsHolder settingsHolder;
        private TransportExtensions<AzureServiceBusTransport> extensions;

        [SetUp]
        public void SetUp()
        {
            settingsHolder = new SettingsHolder();
            new DefaultConfigurationValues().Apply(settingsHolder);
            extensions = new TransportExtensions<AzureServiceBusTransport>(settingsHolder);
        }

        [Test]
        public void Default_should_use_mapper_that_converts_namespace_name_to_connection_string()
        {
            var mapper = settingsHolder.Get<Type>(WellKnownConfigurationKeys.Topology.Addressing.UseLogicalNamespaceName);

            Assert.AreEqual(typeof(DefaultNamespaceNameToConnectionStringMapper), mapper);
        }

        [Test]
        public void Should_use_pass_throught_mapper()
        {
            extensions.UseDefaultTopology().Addressing().UseLogicalNamespaceName();

            var mapper = settingsHolder.Get<Type>(WellKnownConfigurationKeys.Topology.Addressing.UseLogicalNamespaceName);

            Assert.AreEqual(typeof(PassThroughNamespaceNameToConnectionStringMapper), mapper);
        }
    }
}