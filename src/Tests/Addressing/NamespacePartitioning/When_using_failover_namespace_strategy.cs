namespace NServiceBus.AzureServiceBus.Tests
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using NServiceBus.AzureServiceBus.Addressing;
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
            var strategy = new FailOverNamespacePartitioningStrategy(new List<string> { primary, secondary });

            Assert.AreEqual(1, strategy.GetConnectionStrings("endpoint1").Count());
        }

        [Test]
        public void Failover_partitioning_strategy_will_return_primary_namespace_by_default()
        {
            const string primary = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            const string secondary = "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            var strategy = new FailOverNamespacePartitioningStrategy(new List<string> { primary, secondary });

            Assert.AreEqual(primary, strategy.GetConnectionStrings("endpoint1").First());
        }

        [Test]
        public void Failover_partitioning_strategy_will_return_secondary_namespace_when_in_secondary_state()
        {
            const string primary = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            const string secondary = "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            var strategy = new FailOverNamespacePartitioningStrategy(new List<string> { primary, secondary })
            {
                Mode = FailOverMode.Secondary
            };


            Assert.AreEqual(secondary, strategy.GetConnectionStrings("endpoint1").First());
        }

        [Test]
        public void Failover_partitioning_strategy_will_throw_if_no_namespace_defined()
        {
            Assert.Throws<ConfigurationErrorsException>(() => new FailOverNamespacePartitioningStrategy(new List<string>()));
        }

        [Test]
        public void Failover_partitioning_strategy_will_throw_if_no_secondary_namespace_is_provided()
        {
            Assert.Throws<ConfigurationErrorsException>(() => new FailOverNamespacePartitioningStrategy(new List<string>
            {
                "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey"
            }));
        }

        [Test]
        public void Failover_partitioning_strategy_will_throw_if_more_than_primary_and_secondary_namespaces_are_provided()
        {
            Assert.Throws<ConfigurationErrorsException>(() => new FailOverNamespacePartitioningStrategy(new List<string>
            {
                "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey",
                "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey",
                "Endpoint=sb://namespace3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey"
            }));
        }
    }
}