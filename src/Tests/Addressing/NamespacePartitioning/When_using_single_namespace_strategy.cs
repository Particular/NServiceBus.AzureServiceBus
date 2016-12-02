namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Addressing.NamespacePartitioning
{
    using System.Configuration;
    using System.Linq;
    using NUnit.Framework;
    using Transport.AzureServiceBus;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_single_namespace_strategy
    {
        [SetUp]
        public void SetUp()
        {
            strategy = new SinglePartitioning();
            strategy.Initialize(new[]
            {
                new NamespaceInfo(Name, ConnectionString)
            });
        }

        [Test]
        public void Single_partitioning_strategy_will_return_a_single_connectionstring_for_the_purpose_of_creating()
        {
            var namespaces = strategy.GetNamespaces(PartitioningIntent.Creating);

            Assert.AreEqual(1, namespaces.Count());
        }

        [Test]
        public void Single_partitioning_strategy_will_return_a_single_connectionstring_for_the_purpose_of_receiving()
        {
            var namespaces = strategy.GetNamespaces(PartitioningIntent.Receiving);

            Assert.AreEqual(1, namespaces.Count());
        }

        [Test]
        public void Single_partitioning_strategy_will_return_a_single_connectionstring_for_the_purpose_of_sending()
        {
            var namespaces = strategy.GetNamespaces(PartitioningIntent.Sending);

            Assert.AreEqual(1, namespaces.Count());
        }

        [Test]
        public void Single_partitioning_strategy_will_return_configured_namespace_for_any_endpoint_name()
        {
            var namespaces = strategy.GetNamespaces(PartitioningIntent.Creating);

            Assert.AreEqual(new RuntimeNamespaceInfo(Name, ConnectionString), namespaces.Single());
        }

        [Test]
        public void Single_partitioning_strategy_will_throw_if_no_namespace_defined()
        {
            var strategyWhichShouldThrow = new SinglePartitioning();
            Assert.Throws<ConfigurationErrorsException>(() => strategyWhichShouldThrow.Initialize(new NamespaceInfo[0]));
        }

        [Test]
        public void Single_partitioning_strategy_will_throw_if_more_namespaces_defined()
        {
            var namespaceOne = new NamespaceInfo(Name, ConnectionString);
            var namespaceTwo = new NamespaceInfo(OtherName, OtherConnectionString);

            var strategyWhichShouldThrow = new SinglePartitioning();
            Assert.Throws<ConfigurationErrorsException>(() => strategyWhichShouldThrow.Initialize(new[]
            {
                namespaceOne,
                namespaceTwo
            }));
        }

        SinglePartitioning strategy;
        static string ConnectionString = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
        static string OtherConnectionString = "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";

        static string Name = "namespace1";
        static string OtherName = "namespace2";
    }
}