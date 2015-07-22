namespace NServiceBus.AzureServiceBus.Tests
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using NServiceBus.AzureServiceBus.Addressing;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_sharded_strategy_on_multiple_namespaces
    {
        [Test]
        public void Sharded_partitioning_strategy_will_return_a_single_namespace()
        {
            var i = 0;

            var buckets = new List<string>
            {
                "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey",
                "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey",
                "Endpoint=sb://namespace3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey"
            };
            var strategy = new ShardedNamespacePartitioningStrategy(buckets);
            strategy.SetShardingRule(() => i);

            Assert.AreEqual(1, strategy.GetConnectionStrings("endpoint1").Count());
        }

        [Test]
        public void Sharded_partitioning_strategy_will_circle_namespaces()
        {
            var i = 0;

            var buckets = new List<string>
            {
                "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey",
                "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey",
                "Endpoint=sb://namespace3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey"
            };
            var strategy = new ShardedNamespacePartitioningStrategy(buckets);
            strategy.SetShardingRule(() => i);
            
            for (i = 0; i < 3; i++)
            {
                Assert.AreEqual(buckets[i], strategy.GetConnectionStrings("endpoint1").First());
            }
        }

        [Test]
        public void Sharded_partitioning_strategy_will_throw_if_no_namespace_defined()
        {
            Assert.Throws<ConfigurationErrorsException>(() => new ShardedNamespacePartitioningStrategy(new List<string>()));
        }

        [Test]
        public void Sharded_partitioning_strategy_will_throw_if_too_little_namespaces_defined()
        {
            Assert.Throws<ConfigurationErrorsException>(() =>
                new ShardedNamespacePartitioningStrategy(new List<string>
                {
                    "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey"
                }));

        }

    }
}