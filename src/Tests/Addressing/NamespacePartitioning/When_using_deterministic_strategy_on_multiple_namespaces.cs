namespace NServiceBus.AzureServiceBus.Tests
{
    using System.Collections.Generic;
    using System.Configuration;
    using NServiceBus.AzureServiceBus.Addressing;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_deterministic_strategy_on_multiple_namespaces
    {
        [Test]
        public void Deterministic_partitioning_strategy_will_circle_namespaces()
        {
            var i = 0;

            var buckets = new List<string>
            {
                "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey",
                "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey",
                "Endpoint=sb://namespace3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey"
            };
            var strategy = new DeterministicNamespacePartitioningStrategy(buckets);
            strategy.SetAllocationRule(() => i);
            
            for (i = 0; i < 3; i++)
            {
                Assert.AreEqual(buckets[i], strategy.GetConnectionString("endpoint1"));
            }

        }

        [Test]
        public void Deterministic_partitioning_strategy_will_throw_if_no_namespace_defined()
        {
            Assert.Throws<ConfigurationErrorsException>(() => new DeterministicNamespacePartitioningStrategy(new List<string>()));
        }

        [Test]
        public void Deterministic_partitioning_strategy_will_throw_if_too_little_namespaces_defined()
        {
            Assert.Throws<ConfigurationErrorsException>(() =>
                new DeterministicNamespacePartitioningStrategy(new List<string>
                {
                    "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey"
                }));
        }

    }
}