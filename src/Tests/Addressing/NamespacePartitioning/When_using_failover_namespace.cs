namespace NServiceBus.AzureServiceBus.Tests
{
    using System.Collections.Generic;
    using System.Configuration;
    using NServiceBus.AzureServiceBus.Addressing;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_failover_namespace
    {
        [Test]
        public void Failover_partitioning_strategy_will_return_primary_namespace_by_default()
        {
            const string primary = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            const string secondary = "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            var strategy = new FailOverNamespacePartitioningStrategy(new List<string> { primary, secondary });

            Assert.AreEqual(primary, strategy.GetConnectionString("endpoint1"));
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


            Assert.AreEqual(secondary, strategy.GetConnectionString("endpoint1"));
        }

        [Test, ExpectedException(typeof(ConfigurationErrorsException))]
        public void Failover_partitioning_strategy_will_throw_if_no_namespace_defined()
        {
            new FailOverNamespacePartitioningStrategy(new List<string>());
        }

        [Test, ExpectedException(typeof(ConfigurationErrorsException))]
        public void Failover_partitioning_strategy_will_throw_if_too_little_namespaces_defined()
        {
            new FailOverNamespacePartitioningStrategy(new List<string>
            {
                "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey"
            });
        }

        [Test, ExpectedException(typeof(ConfigurationErrorsException))]
        public void Failover_partitioning_strategy_will_throw_if_more_namespaces_defined()
        {
            new FailOverNamespacePartitioningStrategy(new List<string>
            {
                "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey",
                "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey",
                "Endpoint=sb://namespace3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey"
            });
        }
    }
}