namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Addressing.NamespacePartitioning
{
    using System.Configuration;
    using System.Linq;
    using AzureServiceBus.Addressing;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_replicated_strategy_on_multiple_namespaces
    {
        static string Primary = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
        static string Secondary = "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
        static string Tertiary = "Endpoint=sb://namespace3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";

        ReplicatedNamespacePartitioning strategy;

        [SetUp]
        public void SetUp()
        {
            var settings = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(settings);
            extensions.NamespacePartitioning().AddNamespace("namespace1", Primary);
            extensions.NamespacePartitioning().AddNamespace("namespace2", Secondary);
            extensions.NamespacePartitioning().AddNamespace("namespace3", Tertiary);

            strategy = new ReplicatedNamespacePartitioning(settings);
        }

        [Test]
        public void Replicated_partitioning_strategy_will_return_all_connectionstrings_for_purpose_of_sending()
        {
            var namespaces = strategy.GetNamespaces("endpoint1", PartitioningIntent.Sending);

            Assert.AreEqual(3, namespaces.Count());
        }

        [Test]
        public void Replicated_partitioning_strategy_will_return_all_connectionstrings_for_purpose_of_creating()
        {
            var namespaces = strategy.GetNamespaces("endpoint1", PartitioningIntent.Creating);

            Assert.AreEqual(3, namespaces.Count());
        }

        [Test]
        public void Replicated_partitioning_strategy_will_return_all_connectionstrings_for_purpose_of_receiving()
        {
            var namespaces = strategy.GetNamespaces("endpoint1", PartitioningIntent.Receiving);

            Assert.AreEqual(3, namespaces.Count());
        }

        [Test]
        public void Replicated_partitioning_strategy_will_throw_if_no_namespace_are_provided()
        {
            var settings = new SettingsHolder();

            Assert.Throws<ConfigurationErrorsException>(() => new ReplicatedNamespacePartitioning(settings));
        }

        [Test]
        public void Replicated_partitioning_strategy_will_throw_if_too_little_namespaces_are_provided()
        {
            var settings = new SettingsHolder();
            var extensions = new AzureServiceBusTopologySettings(settings);
            extensions.NamespacePartitioning().AddNamespace("namespace1", Primary);

            Assert.Throws<ConfigurationErrorsException>(() => new ReplicatedNamespacePartitioning(settings));
        }

    }
}