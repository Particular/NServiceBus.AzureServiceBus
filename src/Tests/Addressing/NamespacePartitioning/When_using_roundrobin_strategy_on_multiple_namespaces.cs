namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Addressing.NamespacePartitioning
{
    using System.Configuration;
    using System.Linq;
    using NUnit.Framework;
    using Transport.AzureServiceBus;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_roundrobin_strategy_on_multiple_namespaces
    {
        [SetUp]
        public void SetUp()
        {
            var primary = new NamespaceInfo(PrimaryName, PrimaryConnectionString);
            var secondary = new NamespaceInfo(SecondaryName, SecondaryConnectionString);
            var tertiary = new NamespaceInfo(TertiaryName, TertiaryConnectionString);

            strategy = new RoundRobinPartitioning();
            strategy.Initialize(new[]
            {
                primary,
                secondary,
                tertiary
            });
        }

        [Test]
        public void Roundrobin_partitioning_strategy_will_return_a_single_connectionstring_for_purpose_of_sending()
        {
            var namespaces = strategy.GetNamespaces(PartitioningIntent.Sending);

            Assert.AreEqual(1, namespaces.Count());
        }

        [Test]
        public void Roundrobin_partitioning_strategy_will_return_all_connectionstrings_for_purpose_of_creating()
        {
            var namespaces = strategy.GetNamespaces(PartitioningIntent.Creating);

            Assert.AreEqual(3, namespaces.Count());
        }

        [Test]
        public void Roundrobin_partitioning_strategy_will_return_all_connectionstrings_for_purpose_of_receiving()
        {
            var namespaces = strategy.GetNamespaces(PartitioningIntent.Receiving);

            Assert.AreEqual(3, namespaces.Count());
        }

        [Test]
        public void Roundrobin_partitioning_strategy_will_circle_active_namespace()
        {
            Assert.AreEqual(new RuntimeNamespaceInfo(PrimaryName, PrimaryConnectionString), strategy.GetNamespaces(PartitioningIntent.Sending).First());
            Assert.AreEqual(new RuntimeNamespaceInfo(SecondaryName, SecondaryConnectionString), strategy.GetNamespaces(PartitioningIntent.Sending).First());
            Assert.AreEqual(new RuntimeNamespaceInfo(TertiaryName, TertiaryConnectionString), strategy.GetNamespaces(PartitioningIntent.Sending).First());

            Assert.AreEqual(new RuntimeNamespaceInfo(PrimaryName, PrimaryConnectionString), strategy.GetNamespaces(PartitioningIntent.Sending).First());
            Assert.AreEqual(new RuntimeNamespaceInfo(SecondaryName, SecondaryConnectionString), strategy.GetNamespaces(PartitioningIntent.Sending).First());
            Assert.AreEqual(new RuntimeNamespaceInfo(TertiaryName, TertiaryConnectionString), strategy.GetNamespaces(PartitioningIntent.Sending).First());
        }

        [Test]
        public void Roundrobin_partitioning_strategy_will_throw_if_no_namespace_are_provided()
        {
            var strategyWhichShouldThrow = new RoundRobinPartitioning();

            Assert.Throws<ConfigurationErrorsException>(() => strategyWhichShouldThrow.Initialize(new NamespaceInfo[0]));
        }

        [Test]
        public void Roundrobin_partitioning_strategy_will_throw_if_too_little_namespaces_are_provided()
        {
            var strategyWhichShouldThrow = new RoundRobinPartitioning();
            var primary = new NamespaceInfo(PrimaryName, PrimaryConnectionString);

            Assert.Throws<ConfigurationErrorsException>(() => strategyWhichShouldThrow.Initialize(new[]
            {
                primary
            }));
        }

        RoundRobinPartitioning strategy;
        static string PrimaryConnectionString = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
        static string SecondaryConnectionString = "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
        static string TertiaryConnectionString = "Endpoint=sb://namespace3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";

        static string PrimaryName = "namespace1";
        static string SecondaryName = "namespace2";
        static string TertiaryName = "namespace3";
    }
}