namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using System.Collections.Generic;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Configuration.AdvanceExtensibility;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_namespace_partitioning
    {
        [Test]
        public void Should_be_able_to_set_the_partitioning_strategy()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var partitioningSettings = extensions.Topology().Addressing().NamespacePartitioning().UseStrategy<MyNamespacePartitioningStrategy>();

            Assert.AreEqual(typeof(MyNamespacePartitioningStrategy), partitioningSettings.GetSettings().Get<Type>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Strategy));
        }

        [Test]
        public void Should_be_able_to_add_a_namespace()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            var @namespace = "Endpoint=sb://namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";

            var partitioningSettings = extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(@namespace);

            Assert.Contains(@namespace, partitioningSettings.GetSettings().Get<List<String>>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces));
        }

        class MyNamespacePartitioningStrategy : INamespacePartitioningStrategy
        {
            public IEnumerable<NamespaceInfo> GetNamespaceInfo(string endpointName)
            {
                throw new NotImplementedException(); // not relevant for the test
            }
        }


    }
}