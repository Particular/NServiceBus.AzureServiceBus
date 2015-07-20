namespace NServiceBus.AzureServiceBus.Tests
{
    using System.Collections.Generic;
    using System.Configuration;
    using NServiceBus.AzureServiceBus.Addressing;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_single_namespace
    {
        [Test]
        public void Single_partitioning_strategy_will_return_configured_namespace()
        {
            const string connectionstring = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            var strategy = new SingleNamespacePartitioningStrategy(new List<string> { connectionstring });

            Assert.AreEqual(connectionstring, strategy.GetConnectionString());
        }

        [Test, ExpectedException(typeof(ConfigurationErrorsException))]
        public void Single_partitioning_strategy_will_throw_if_no_namespace_defined()
        {
            new SingleNamespacePartitioningStrategy(new List<string>());
        }

        [Test, ExpectedException(typeof(ConfigurationErrorsException))]
        public void Single_partitioning_strategy_will_throw_if_more_namespaces_defined()
        {
            new SingleNamespacePartitioningStrategy(new List<string>
            {
                "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey",
                "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey"
            });
        }
    }
}