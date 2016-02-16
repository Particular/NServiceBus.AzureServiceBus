namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_use_logical_namespace_name
    {
        private SettingsHolder settingsHolder;
        private TransportExtensions<AzureServiceBusTransport> extensions;
        private NamespaceInfo namespaceInfo;

        [SetUp]
        public void SetUp()
        {
            settingsHolder = new SettingsHolder();
            new DefaultConfigurationValues().Apply(settingsHolder);
            extensions = new TransportExtensions<AzureServiceBusTransport>(settingsHolder);

            namespaceInfo = new NamespaceInfo("name", "connection string");
        }

        [Test]
        public void Default_should_use_namespace_connection_string()
        {
            var resolver = settingsHolder.Get<Func<NamespaceInfo, string>>(WellKnownConfigurationKeys.Topology.Addressing.UseLogicalNamespaceName);

            Assert.AreEqual("connection string", resolver(namespaceInfo));
        }

        [Test]
        public void Should_use_namespace_name()
        {
            extensions.UseDefaultTopology().Addressing().UseLogicalNamespaceName();

            var resolver = settingsHolder.Get<Func<NamespaceInfo, string>>(WellKnownConfigurationKeys.Topology.Addressing.UseLogicalNamespaceName);

            Assert.AreEqual("name", resolver(namespaceInfo));
        }
    }
}