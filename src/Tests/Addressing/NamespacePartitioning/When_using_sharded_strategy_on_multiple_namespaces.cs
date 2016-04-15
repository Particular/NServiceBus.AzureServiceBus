namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Addressing.NamespacePartitioning
{
    using System.Configuration;
    using System.Linq;
    using AzureServiceBus;
    using AzureServiceBus.Addressing;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_sharded_strategy_on_multiple_namespaces
    {
        static string ConnectionString1 = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
        static string ConnectionString2 = "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
        static string ConnectionString3 = "Endpoint=sb://namespace3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";

        static string Name1 = "namespace1";
        static string Name2 = "namespace2";
        static string Name3 = "namespace3";

        ShardedNamespacePartitioning strategy;

        [SetUp]
        public void SetUp()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.NamespacePartitioning().AddNamespace(Name1, ConnectionString1);
            extensions.NamespacePartitioning().AddNamespace(Name2, ConnectionString2);
            extensions.NamespacePartitioning().AddNamespace(Name3, ConnectionString3);

            strategy = new ShardedNamespacePartitioning(settings);
        }

        [Test]
        public void Sharded_partitioning_strategy_will_return_a_single_namespace_for_the_purpose_of_sending()
        {
            strategy.SetShardingRule(() => 0);

            var namespaces = strategy.GetNamespaces("endpoint1", PartitioningIntent.Sending);
            Assert.AreEqual(1, namespaces.Count());
        }

        [Test]
        public void Sharded_partitioning_strategy_will_return_all_namespaces_for_the_purpose_of_creation()
        {
            strategy.SetShardingRule(() => 0);

            var namespaces = strategy.GetNamespaces("endpoint1", PartitioningIntent.Creating);
            Assert.AreEqual(3, namespaces.Count());
        }

        [Test]
        public void Sharded_partitioning_strategy_will_return_all_namespaces_for_the_purpose_of_receiving()
        {
            strategy.SetShardingRule(() => 0);

            var namespaces = strategy.GetNamespaces("endpoint1", PartitioningIntent.Receiving);
            Assert.AreEqual(3, namespaces.Count());
        }

        [Test]
        public void Sharded_partitioning_strategy_will_circle__the_active_namespace()
        {
            strategy.SetShardingRule(() => 0);
            Assert.AreEqual(new RuntimeNamespaceInfo(Name1, ConnectionString1, NamespaceMode.Active), strategy.GetNamespaces("endpoint1", PartitioningIntent.Sending).First());
            strategy.SetShardingRule(() => 1);
            Assert.AreEqual(new RuntimeNamespaceInfo(Name2, ConnectionString2, NamespaceMode.Active), strategy.GetNamespaces("endpoint1", PartitioningIntent.Sending).First());
            strategy.SetShardingRule(() => 2);
            Assert.AreEqual(new RuntimeNamespaceInfo(Name3, ConnectionString3, NamespaceMode.Active), strategy.GetNamespaces("endpoint1", PartitioningIntent.Sending).First());
        }

        [Test]
        public void Sharded_partitioning_strategy_will_throw_if_no_namespace_defined()
        {
            var settings = new SettingsHolder();

            Assert.Throws<ConfigurationErrorsException>(() => new ShardedNamespacePartitioning(settings));
        }

        [Test]
        public void Sharded_partitioning_strategy_will_throw_if_too_little_namespaces_defined()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.NamespacePartitioning().AddNamespace(Name1, ConnectionString1);

            Assert.Throws<ConfigurationErrorsException>(() => new ShardedNamespacePartitioning(settings));

        }

    }
}