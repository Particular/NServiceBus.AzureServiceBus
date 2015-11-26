namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_sharded_strategy_on_multiple_namespaces
    {
        [Test]
        public void Sharded_partitioning_strategy_will_return_a_single_namespace_for_the_purpose_of_sending()
        {
            var i = 0;

            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace("Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey");
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace("Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey");
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace("Endpoint=sb://namespace3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey");

            var strategy = new ShardedNamespacePartitioningStrategy(settings);
            strategy.SetShardingRule(() => i);

            Assert.AreEqual(1, strategy.GetNamespaces("endpoint1", PartitioningIntent.Sending).Count());
        }

        [Test]
        public void Sharded_partitioning_strategy_will_return_all_namespaces_for_the_purpose_of_creation()
        {
            var i = 0;

            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace("Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey");
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace("Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey");
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace("Endpoint=sb://namespace3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey");

            var strategy = new ShardedNamespacePartitioningStrategy(settings);
            strategy.SetShardingRule(() => i);

            Assert.AreEqual(3, strategy.GetNamespaces("endpoint1", PartitioningIntent.Creating).Count());
        }

        [Test]
        public void Sharded_partitioning_strategy_will_return_all_namespaces_for_the_purpose_of_receiving()
        {
            var i = 0;

            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace("Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey");
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace("Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey");
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace("Endpoint=sb://namespace3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey");

            var strategy = new ShardedNamespacePartitioningStrategy(settings);
            strategy.SetShardingRule(() => i);

            Assert.AreEqual(3, strategy.GetNamespaces("endpoint1", PartitioningIntent.Receiving).Count());
        }

        [Test]
        public void Sharded_partitioning_strategy_will_circle__the_active_namespace()
        {
            var i = 0;

            var buckets = new List<String>()
            {
                "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey",
                "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey",
                "Endpoint=sb://namespace3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey"
            };

            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            foreach (var s in buckets)
            {
                extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace(s);
            }

            var strategy = new ShardedNamespacePartitioningStrategy(settings);
            strategy.SetShardingRule(() => i);
            
            for (i = 0; i < 3; i++)
            {
                Assert.AreEqual(new NamespaceInfo(buckets[i], NamespaceMode.Active), strategy.GetNamespaces("endpoint1", PartitioningIntent.Sending).First());
            }
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
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace("Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey");

            Assert.Throws<ConfigurationErrorsException>(() =>
                new ShardedNamespacePartitioningStrategy(settings));

        }

    }
}