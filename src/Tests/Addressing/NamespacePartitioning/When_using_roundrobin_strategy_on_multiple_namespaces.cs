namespace NServiceBus.AzureServiceBus.Tests
{
    using System.Configuration;
    using System.Linq;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_roundrobin_strategy_on_multiple_namespaces
    {
        [Test]
        public void Roundrobin_partitioning_strategy_will_return_a_single_connectionstring_for_purpose_of_sending()
        {
            const string primary = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            const string secondary = "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            const string tertiary = "Endpoint=sb://namespace3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";

            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(primary);
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(secondary);
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(tertiary);

            var strategy = new RoundRobinNamespacePartitioningStrategy(settings);

            Assert.AreEqual(1, strategy.GetNamespaces("endpoint1", PartitioningIntent.Sending).Count());
        }

        [Test]
        public void Roundrobin_partitioning_strategy_will_return_all_connectionstrings_for_purpose_of_creating()
        {
            const string primary = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            const string secondary = "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            const string tertiary = "Endpoint=sb://namespace3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";

            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(primary);
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(secondary);
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(tertiary);

            var strategy = new RoundRobinNamespacePartitioningStrategy(settings);

            Assert.AreEqual(3, strategy.GetNamespaces("endpoint1", PartitioningIntent.Creating).Count());
        }

        [Test]
        public void Roundrobin_partitioning_strategy_will_return_all_connectionstrings_for_purpose_of_receiving()
        {
            const string primary = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            const string secondary = "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            const string tertiary = "Endpoint=sb://namespace3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";

            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(primary);
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(secondary);
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(tertiary);

            var strategy = new RoundRobinNamespacePartitioningStrategy(settings);

            Assert.AreEqual(3, strategy.GetNamespaces("endpoint1", PartitioningIntent.Receiving).Count());
        }

        [Test]
        public void Roundrobin_partitioning_strategy_will_circle_active_namespace()
        {
            const string primary = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            const string secondary = "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            const string tertiary = "Endpoint=sb://namespace3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";

            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(primary);
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(secondary);
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(tertiary);

            var strategy = new RoundRobinNamespacePartitioningStrategy(settings);

            for (var i = 0; i < 2; i++)
            {
                Assert.AreEqual(new NamespaceInfo(primary, NamespaceMode.Active), strategy.GetNamespaces("endpoint1", PartitioningIntent.Sending).First());
                Assert.AreEqual(new NamespaceInfo(secondary, NamespaceMode.Active), strategy.GetNamespaces("endpoint1", PartitioningIntent.Sending).First());
                Assert.AreEqual(new NamespaceInfo(tertiary, NamespaceMode.Active), strategy.GetNamespaces("endpoint1", PartitioningIntent.Sending).First());
            }
        }
        
        [Test]
        public void Roundrobin_partitioning_strategy_will_throw_if_no_namespace_are_provided()
        {
            var settings = new SettingsHolder();

            Assert.Throws<ConfigurationErrorsException>(() => new RoundRobinNamespacePartitioningStrategy(settings));
        }

        [Test]
        public void Roundrobin_partitioning_strategy_will_throw_if_too_little_namespaces_are_provided()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace("Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey");
            
            Assert.Throws<ConfigurationErrorsException>(() => new RoundRobinNamespacePartitioningStrategy(settings));
        }

    }
}