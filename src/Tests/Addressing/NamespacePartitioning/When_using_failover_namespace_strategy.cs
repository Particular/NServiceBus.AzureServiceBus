namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Addressing.NamespacePartitioning
{
    using System.Configuration;
    using System.Linq;
    using AzureServiceBus;
    using AzureServiceBus.Addressing;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_failover_namespace_strategy
    {
        const string PrimaryConnectionString = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
        const string SecondaryConnectionString = "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
        const string OtherConnectionString = "Endpoint=sb://namespace3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";

        const string PrimaryName = "namespace1";
        const string SecondaryName = "namespace2";
        const string OtherName = "namespace3";

        FailOverNamespacePartitioning strategy;

        [SetUp]
        public void SetUp()
        {
            var settings = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(settings);
            extensions.NamespacePartitioning().AddNamespace(PrimaryName, PrimaryConnectionString);
            extensions.NamespacePartitioning().AddNamespace(SecondaryName, SecondaryConnectionString);

            strategy = new FailOverNamespacePartitioning(settings);
        }

        [Test]
        public void Failover_partitioning_strategy_will_return_a_single_active_namespace_for_the_purpose_of_sending()
        {
            var namespaces = strategy.GetNamespaces("endpoint1", PartitioningIntent.Sending);

            Assert.AreEqual(1, namespaces.Count());
        }

        [Test]
        public void Failover_partitioning_strategy_will_return_both_namespaces_for_the_purpose_of_receiving()
        {
            var namespaces = strategy.GetNamespaces("endpoint1", PartitioningIntent.Receiving);

            Assert.AreEqual(2, namespaces.Count());
        }

        [Test]
        public void Failover_partitioning_strategy_will_both_namespaces_for_the_purpose_of_creation()
        {
            var namespaces = strategy.GetNamespaces("endpoint1", PartitioningIntent.Creating);

            Assert.AreEqual(2, namespaces.Count());
        }

        [Test]
        public void Failover_partitioning_strategy_will_return_primary_namespace_by_default()
        {
            var namespaces = strategy.GetNamespaces("endpoint1", PartitioningIntent.Creating);

            Assert.AreEqual(new RuntimeNamespaceInfo(PrimaryName, PrimaryConnectionString, NamespaceMode.Active), namespaces.First());
        }

        [Test]
        public void Failover_partitioning_strategy_will_return_secondary_namespace_when_in_secondary_state()
        {
            strategy.Mode = FailOverMode.Secondary;

            var namespaces = strategy.GetNamespaces("endpoint1", PartitioningIntent.Sending);

            Assert.AreEqual(new RuntimeNamespaceInfo(SecondaryName, SecondaryConnectionString, NamespaceMode.Active), namespaces.First());
        }

        [Test]
        public void Failover_partitioning_strategy_will_throw_if_no_namespace_defined()
        {
            var settings = new SettingsHolder();

            Assert.Throws<ConfigurationErrorsException>(() => new FailOverNamespacePartitioning(settings));
        }

        [Test]
        public void Failover_partitioning_strategy_will_throw_if_no_secondary_namespace_is_provided()
        {
            var settings = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(settings);
            extensions.NamespacePartitioning().AddNamespace(PrimaryName, PrimaryConnectionString);

            Assert.Throws<ConfigurationErrorsException>(() => new FailOverNamespacePartitioning(settings));
        }

        [Test]
        public void Failover_partitioning_strategy_will_throw_if_more_than_primary_and_secondary_namespaces_are_provided()
        {
            var settings = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(settings);
            extensions.NamespacePartitioning().AddNamespace(PrimaryName, PrimaryConnectionString);
            extensions.NamespacePartitioning().AddNamespace(SecondaryName, SecondaryConnectionString);
            extensions.NamespacePartitioning().AddNamespace(OtherName, OtherConnectionString);

            Assert.Throws<ConfigurationErrorsException>(() => new FailOverNamespacePartitioning(settings));
        }
    }
}