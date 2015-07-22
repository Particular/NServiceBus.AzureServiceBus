namespace NServiceBus.AzureServiceBus.Tests
{
    using System.Collections.Generic;
    using System.Configuration;
    using NServiceBus.AzureServiceBus.Addressing;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_roundrobin_strategy_on_multiple_namespaces
    {
        [Test]
        public void Roundrobin_partitioning_strategy_will_circle_namespaces()
        {
            const string primary = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            const string secondary = "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            const string tertiary = "Endpoint=sb://namespace3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            var strategy = new RoundRobinNamespacePartitioningStrategy(new List<string>{ primary, secondary, tertiary});

            for (var i = 0; i < 2; i++)
            {
                Assert.AreEqual(primary, strategy.GetConnectionString("endpoint1"));
                Assert.AreEqual(secondary, strategy.GetConnectionString("endpoint1"));
                Assert.AreEqual(tertiary, strategy.GetConnectionString("endpoint1"));
            }
        }
        
        [Test]
        public void Roundrobin_partitioning_strategy_will_throw_if_no_namespace_are_provided()
        {
            Assert.Throws<ConfigurationErrorsException>(() => new RoundRobinNamespacePartitioningStrategy(new List<string>()));
        }

        [Test]
        public void Roundrobin_partitioning_strategy_will_throw_if_too_little_namespaces_are_provided()
        {
            Assert.Throws<ConfigurationErrorsException>(() => new RoundRobinNamespacePartitioningStrategy(new List<string>
            {
                "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey"
            }));
        }

    }
}