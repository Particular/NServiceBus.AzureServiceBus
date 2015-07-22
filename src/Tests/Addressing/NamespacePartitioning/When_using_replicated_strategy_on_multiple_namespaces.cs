namespace NServiceBus.AzureServiceBus.Tests
{
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using NServiceBus.AzureServiceBus.Addressing;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_replicated_strategy_on_multiple_namespaces
    {
        [Test]
        public void Replicated_partitioning_strategy_will_return_all_connectionstrings()
        {
            const string primary = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            const string secondary = "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            const string tertiary = "Endpoint=sb://namespace3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            var strategy = new ReplicatedNamespacePartitioningStrategy(new List<string> { primary, secondary, tertiary });

            Assert.AreEqual(3, strategy.GetConnectionStrings("endpoint1").Count());
        }

        [Test]
        public void Replicated_partitioning_strategy_will_throw_if_no_namespace_are_provided()
        {
            Assert.Throws<ConfigurationErrorsException>(() => new ReplicatedNamespacePartitioningStrategy(new List<string>()));
        }

        [Test]
        public void Replicated_partitioning_strategy_will_throw_if_too_little_namespaces_are_provided()
        {
            Assert.Throws<ConfigurationErrorsException>(() => new ReplicatedNamespacePartitioningStrategy(new List<string>
            {
                "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey"
            }));
        }

    }
}