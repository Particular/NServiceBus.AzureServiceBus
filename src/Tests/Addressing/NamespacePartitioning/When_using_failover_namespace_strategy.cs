namespace NServiceBus.AzureServiceBus.Tests
{
    using System.Configuration;
    using System.Linq;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_failover_namespace_strategy
    {
        [Test]
        public void Failover_partitioning_strategy_will_return_a_single_namespace()
        {
            const string primary = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            const string secondary = "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";

            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(primary);
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(secondary);

            var strategy = new FailOverNamespacePartitioningStrategy(settings);

            Assert.AreEqual(1, strategy.GetNamespaceInfo("endpoint1").Count());
        }

        [Test]
        public void Failover_partitioning_strategy_will_return_primary_namespace_by_default()
        {
            const string primary = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            const string secondary = "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";

            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(primary);
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(secondary);

            var strategy = new FailOverNamespacePartitioningStrategy(settings);


            Assert.AreEqual(new NamespaceInfo(primary, NamespaceMode.Active), strategy.GetNamespaceInfo("endpoint1").First());
        }

        [Test]
        public void Failover_partitioning_strategy_will_return_secondary_namespace_when_in_secondary_state()
        {
            const string primary = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            const string secondary = "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(primary);
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(secondary);

            var strategy = new FailOverNamespacePartitioningStrategy(settings)
            {
                Mode = FailOverMode.Secondary
            };


            Assert.AreEqual(new NamespaceInfo(secondary, NamespaceMode.Passive), strategy.GetNamespaceInfo("endpoint1").First());
        }

        [Test]
        public void Failover_partitioning_strategy_will_throw_if_no_namespace_defined()
        {
            var settings = new SettingsHolder();

            Assert.Throws<ConfigurationErrorsException>(() => new FailOverNamespacePartitioningStrategy(settings));
        }

        [Test]
        public void Failover_partitioning_strategy_will_throw_if_no_secondary_namespace_is_provided()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace("Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey");
            
            Assert.Throws<ConfigurationErrorsException>(() => new FailOverNamespacePartitioningStrategy(settings));
        }

        [Test]
        public void Failover_partitioning_strategy_will_throw_if_more_than_primary_and_secondary_namespaces_are_provided()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace("Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey");
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace("Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey");
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace("Endpoint=sb://namespace3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey");

            Assert.Throws<ConfigurationErrorsException>(() => new FailOverNamespacePartitioningStrategy(settings));
        }
    }
}