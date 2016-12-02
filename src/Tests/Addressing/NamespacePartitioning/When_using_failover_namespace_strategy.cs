namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Addressing.NamespacePartitioning
{
    using System.Configuration;
    using System.Linq;
    using NUnit.Framework;
    using Transport.AzureServiceBus;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_failover_namespace_strategy
    {
        [SetUp]
        public void SetUp()
        {
            var primary = new NamespaceInfo(PrimaryName, PrimaryConnectionString);
            var secondary = new NamespaceInfo(SecondaryName, SecondaryConnectionString);

            strategy = new FailOverPartitioning();
            strategy.Initialize(new[]
            {
                primary,
                secondary
            });
        }

        [Test]
        public void Failover_partitioning_strategy_will_return_a_all_namespace_for_the_purpose_of_sending()
        {
            var namespaces = strategy.GetNamespaces(PartitioningIntent.Sending);

            Assert.AreEqual(2, namespaces.Count());
        }

        [Test]
        public void Failover_partitioning_strategy_will_return_both_namespaces_for_the_purpose_of_receiving()
        {
            var namespaces = strategy.GetNamespaces(PartitioningIntent.Receiving);

            Assert.AreEqual(2, namespaces.Count());
        }

        [Test]
        public void Failover_partitioning_strategy_will_both_namespaces_for_the_purpose_of_creation()
        {
            var namespaces = strategy.GetNamespaces(PartitioningIntent.Creating);

            Assert.AreEqual(2, namespaces.Count());
        }

        [Test]
        public void Failover_partitioning_strategy_will_return_primary_namespace_by_default()
        {
            var namespaces = strategy.GetNamespaces(PartitioningIntent.Creating);

            Assert.AreEqual(new RuntimeNamespaceInfo(PrimaryName, PrimaryConnectionString), namespaces.First());
        }

        [Test]
        public void Failover_partitioning_strategy_will_return_primary_and_secondary_namespace_when_in_secondary_state()
        {
            strategy.Mode = FailOverMode.Secondary;

            var namespaces = strategy.GetNamespaces(PartitioningIntent.Sending);

            Assert.AreEqual(new RuntimeNamespaceInfo(SecondaryName, SecondaryConnectionString), namespaces.Last());
        }

        [Test]
        public void Failover_partitioning_strategy_will_throw_if_no_namespace_defined()
        {
            var strategyWhichShouldThrow = new FailOverPartitioning();

            Assert.Throws<ConfigurationErrorsException>(() => strategyWhichShouldThrow.Initialize(new NamespaceInfo[0]));
        }

        [Test]
        public void Failover_partitioning_strategy_will_throw_if_no_secondary_namespace_is_provided()
        {
            var strategyWhichShouldThrow = new FailOverPartitioning();
            var primary = new NamespaceInfo(PrimaryName, PrimaryConnectionString);

            Assert.Throws<ConfigurationErrorsException>(() => strategyWhichShouldThrow.Initialize(new[]
            {
                primary
            }));
        }

        [Test]
        public void Failover_partitioning_strategy_will_throw_if_more_than_primary_and_secondary_namespaces_are_provided()
        {
            var strategyWhichShouldThrow = new FailOverPartitioning();
            var primary = new NamespaceInfo(PrimaryName, PrimaryConnectionString);
            var secondary = new NamespaceInfo(SecondaryName, SecondaryConnectionString);
            var another = new NamespaceInfo(OtherName, OtherConnectionString);

            Assert.Throws<ConfigurationErrorsException>(() => strategyWhichShouldThrow.Initialize(new[]
            {
                primary,
                secondary,
                another
            }));
        }

        FailOverPartitioning strategy;
        const string PrimaryConnectionString = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
        const string SecondaryConnectionString = "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
        const string OtherConnectionString = "Endpoint=sb://namespace3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";

        const string PrimaryName = "namespace1";
        const string SecondaryName = "namespace2";
        const string OtherName = "namespace3";
    }
}