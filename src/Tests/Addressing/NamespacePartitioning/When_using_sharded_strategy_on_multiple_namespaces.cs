namespace NServiceBus.AzureServiceBus.Tests
{
    using System.Configuration;
    using System.Linq;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_sharded_strategy_on_multiple_namespaces
    {
        private static readonly string ConnectionString1 = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
        private static readonly string ConnectionString2 = "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
        private static readonly string ConnectionString3 = "Endpoint=sb://namespace3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";

        private static readonly string Name1 = "namespace1";
        private static readonly string Name2 = "namespace2";
        private static readonly string Name3 = "namespace3";

        private ShardedNamespacePartitioningStrategy _strategy;

        [SetUp]
        public void SetUp()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace(Name1, ConnectionString1);
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace(Name2, ConnectionString2);
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace(Name3, ConnectionString3);

            _strategy = new ShardedNamespacePartitioningStrategy(settings);
        }

        [Test]
        public void Sharded_partitioning_strategy_will_return_a_single_namespace_for_the_purpose_of_sending()
        {
            _strategy.SetShardingRule(() => 0);

            var namespaces = _strategy.GetNamespaces("endpoint1", PartitioningIntent.Sending);
            Assert.AreEqual(1, namespaces.Count());
        }

        [Test]
        public void Sharded_partitioning_strategy_will_return_all_namespaces_for_the_purpose_of_creation()
        {
            _strategy.SetShardingRule(() => 0);

            var namespaces = _strategy.GetNamespaces("endpoint1", PartitioningIntent.Creating);
            Assert.AreEqual(3, namespaces.Count());
        }

        [Test]
        public void Sharded_partitioning_strategy_will_return_all_namespaces_for_the_purpose_of_receiving()
        {
            _strategy.SetShardingRule(() => 0);

            var namespaces = _strategy.GetNamespaces("endpoint1", PartitioningIntent.Receiving);
            Assert.AreEqual(3, namespaces.Count());
        }

        [Test]
        public void Sharded_partitioning_strategy_will_circle__the_active_namespace()
        {
            _strategy.SetShardingRule(() => 0);
            Assert.AreEqual(new NamespaceInfo(Name1, ConnectionString1, NamespaceMode.Active), _strategy.GetNamespaces("endpoint1", PartitioningIntent.Sending).First());
            _strategy.SetShardingRule(() => 1);
            Assert.AreEqual(new NamespaceInfo(Name2, ConnectionString2, NamespaceMode.Active), _strategy.GetNamespaces("endpoint1", PartitioningIntent.Sending).First());
            _strategy.SetShardingRule(() => 2);
            Assert.AreEqual(new NamespaceInfo(Name3, ConnectionString3, NamespaceMode.Active), _strategy.GetNamespaces("endpoint1", PartitioningIntent.Sending).First());
        }

        [Test]
        public void Sharded_partitioning_strategy_will_throw_if_no_namespace_defined()
        {
            var settings = new SettingsHolder();

            Assert.Throws<ConfigurationErrorsException>(() => new ShardedNamespacePartitioningStrategy(settings));
        }

        [Test]
        public void Sharded_partitioning_strategy_will_throw_if_too_little_namespaces_defined()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace(Name1, ConnectionString1);

            Assert.Throws<ConfigurationErrorsException>(() => new ShardedNamespacePartitioningStrategy(settings));

        }

    }
}