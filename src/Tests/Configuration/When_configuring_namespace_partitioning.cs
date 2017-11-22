namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Configuration
{
    using System;
    using System.Collections.Generic;
    using Transport.AzureServiceBus;
    using NServiceBus.Configuration.AdvancedExtensibility;
    using Settings;
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

            var partitioningSettings = extensions.NamespacePartitioning().UseStrategy<MyNamespacePartitioningStrategy>();

            Assert.AreEqual(typeof(MyNamespacePartitioningStrategy), partitioningSettings.GetSettings().Get<Type>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Strategy));
        }

        [Test]
        public void Should_be_able_to_add_a_namespace()
        {
            const string connectionString = "Endpoint=sb://namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            const string name = "namespace1";

            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.NamespacePartitioning().AddNamespace(name, connectionString);

            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);

            CollectionAssert.Contains(namespacesDefinition, new NamespaceInfo(name, connectionString));
        }

        class MyNamespacePartitioningStrategy : INamespacePartitioningStrategy
        {
            public bool SendingNamespacesCanBeCached { get; }

            public IEnumerable<RuntimeNamespaceInfo> GetNamespaces(PartitioningIntent partitioningIntent)
            {
                throw new NotImplementedException(); // not relevant for the test
            }
        }
    }
}