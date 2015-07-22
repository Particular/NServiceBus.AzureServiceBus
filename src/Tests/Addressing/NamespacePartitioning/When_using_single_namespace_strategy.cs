namespace NServiceBus.AzureServiceBus.Tests
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using NServiceBus.AzureServiceBus.Addressing;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_single_namespace_strategy
    {
        [Test]
        public void Single_partitioning_strategy_will_return_a_single_connectionstring()
        {
            const string connectionstring = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            var strategy = new SingleNamespacePartitioningStrategy(new List<string> { connectionstring });

            Assert.AreEqual(1, strategy.GetConnectionStrings("endpoint1").Count());
        }

        [Test]
        public void Single_partitioning_strategy_will_return_configured_namespace_for_any_endpoint_name()
        {
            const string connectionstring = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            var strategy = new SingleNamespacePartitioningStrategy(new List<string> { connectionstring });

            Assert.AreEqual(connectionstring, strategy.GetConnectionStrings("endpoint1").First());
        }

        [Test]
        public void Single_partitioning_strategy_will_throw_if_no_namespace_defined()
        {
            Assert.Throws<ConfigurationErrorsException>(() => new SingleNamespacePartitioningStrategy(new List<string>()));
        }

        [Test]
        public void Single_partitioning_strategy_will_throw_if_more_namespaces_defined()
        {
            Assert.Throws<ConfigurationErrorsException>(() =>
                new SingleNamespacePartitioningStrategy(new List<string>
                {
                    "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey",
                    "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey"
                }));
        }
    }

}