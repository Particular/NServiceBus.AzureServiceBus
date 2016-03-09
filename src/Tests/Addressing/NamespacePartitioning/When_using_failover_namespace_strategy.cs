namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Addressing.NamespacePartitioning
{
    using System.Configuration;
    using System.Linq;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_failover_namespace_strategy
    {
        private static readonly string PrimaryConnectionString = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
        private static readonly string SecondaryConnectionString = "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
        private static readonly string OtherConnectionString = "Endpoint=sb://namespace3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";

        private static readonly string PrimaryName = "namespace1";
        private static readonly string SecondaryName = "namespace2";
        private static readonly string OtherName = "namespace3";
        
        private FailOverNamespacePartitioningStrategy _strategy;

        [SetUp]
        public void SetUp()
        {
            var settings = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(settings);
            extensions.Addressing().NamespacePartitioning().AddNamespace(PrimaryName, PrimaryConnectionString);
            extensions.Addressing().NamespacePartitioning().AddNamespace(SecondaryName, SecondaryConnectionString);

            _strategy = new FailOverNamespacePartitioningStrategy(settings);
        }

        [Test]
        public void Failover_partitioning_strategy_will_return_a_single_active_namespace_for_the_purpose_of_sending()
        {
            var namespaces = _strategy.GetNamespaces("endpoint1", PartitioningIntent.Sending);

            Assert.AreEqual(1, namespaces.Count());
        }

        [Test]
        public void Failover_partitioning_strategy_will_return_both_namespaces_for_the_purpose_of_receiving()
        {
            var namespaces = _strategy.GetNamespaces("endpoint1", PartitioningIntent.Receiving);

            Assert.AreEqual(2, namespaces.Count());
        }

        [Test]
        public void Failover_partitioning_strategy_will_both_namespaces_for_the_purpose_of_creation()
        {
            var namespaces = _strategy.GetNamespaces("endpoint1", PartitioningIntent.Creating);

            Assert.AreEqual(2, namespaces.Count());
        }

        [Test]
        public void Failover_partitioning_strategy_will_return_primary_namespace_by_default()
        {
            var namespaces = _strategy.GetNamespaces("endpoint1", PartitioningIntent.Creating);

            Assert.AreEqual(new RuntimeNamespaceInfo(PrimaryName, PrimaryConnectionString, NamespaceMode.Active), namespaces.First());
        }

        [Test]
        public void Failover_partitioning_strategy_will_return_secondary_namespace_when_in_secondary_state()
        {
            _strategy.Mode = FailOverMode.Secondary;

            var namespaces = _strategy.GetNamespaces("endpoint1", PartitioningIntent.Sending);

            Assert.AreEqual(new RuntimeNamespaceInfo(SecondaryName, SecondaryConnectionString, NamespaceMode.Active), namespaces.First());
        }

        [Test]
        public void Failover_partitioning_strategy_will_throw_if_no_namespace_defined()
        {
            var settings = new SettingsHolder();

            Assert.Throws<ConfigurationErrorsException>(() => new FailOverNamespacePartitioningStrategy(settings));
        }

        [Test]
        public void Failover_partitioning_strategy_will_throw_if_no_secondary_namespace_is_provided()
        {
            var settings = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(settings);
            extensions.Addressing().NamespacePartitioning().AddNamespace(PrimaryName, PrimaryConnectionString);
            
            Assert.Throws<ConfigurationErrorsException>(() => new FailOverNamespacePartitioningStrategy(settings));
        }

        [Test]
        public void Failover_partitioning_strategy_will_throw_if_more_than_primary_and_secondary_namespaces_are_provided()
        {
            var settings = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(settings);
            extensions.Addressing().NamespacePartitioning().AddNamespace(PrimaryName, PrimaryConnectionString);
            extensions.Addressing().NamespacePartitioning().AddNamespace(SecondaryName, SecondaryConnectionString);
            extensions.Addressing().NamespacePartitioning().AddNamespace(OtherName, OtherConnectionString);

            Assert.Throws<ConfigurationErrorsException>(() => new FailOverNamespacePartitioningStrategy(settings));
        }
    }
}