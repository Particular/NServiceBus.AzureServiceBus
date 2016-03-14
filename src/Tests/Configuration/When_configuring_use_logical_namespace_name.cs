namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using System;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.AzureServiceBus.Topology.MetaModel;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_use_logical_namespace_name
    {
        private SettingsHolder settingsHolder;
        private AzureServiceBusTopologySettings extensions;

        [SetUp]
        public void SetUp()
        {
            settingsHolder = new SettingsHolder();
            new DefaultConfigurationValues().Apply(settingsHolder);
            extensions = new AzureServiceBusTopologySettings(settingsHolder);
        }

        [Test]
        public void Default_should_use_mapper_that_converts_namespace_name_to_connection_string()
        {
            var mapper = settingsHolder.Get<Type>(WellKnownConfigurationKeys.Topology.Addressing.UseNamespaceNamesInsteadOfConnectionStrings);

            Assert.AreEqual(typeof(DefaultNamespaceNameToConnectionStringMapper), mapper);
        }

        [Test]
        public void Should_use_pass_through_mapper()
        {
            extensions.UseNamespaceNamesInsteadOfConnectionStrings();

            var mapper = settingsHolder.Get<Type>(WellKnownConfigurationKeys.Topology.Addressing.UseNamespaceNamesInsteadOfConnectionStrings);

            Assert.AreEqual(typeof(PassThroughNamespaceNameToConnectionStringMapper), mapper);
        }
    }
}