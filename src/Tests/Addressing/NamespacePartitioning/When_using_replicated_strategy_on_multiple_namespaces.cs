namespace NServiceBus.AzureServiceBus.Tests
{
    using System.Configuration;
    using System.Linq;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_replicated_strategy_on_multiple_namespaces
    {
        [Test]
        public void Replicated_partitioning_strategy_will_return_all_connectionstrings_for_purpose_of_sending()
        {
            const string primary = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            const string secondary = "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            const string tertiary = "Endpoint=sb://namespace3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";

            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace(primary);
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace(secondary);
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace(tertiary);

            var strategy = new ReplicatedNamespacePartitioningStrategy(settings);

            Assert.AreEqual(3, strategy.GetNamespaces("endpoint1", PartitioningIntent.Sending).Count());
        }

        [Test]
        public void Replicated_partitioning_strategy_will_return_all_connectionstrings_for_purpose_of_creating()
        {
            const string primary = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            const string secondary = "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            const string tertiary = "Endpoint=sb://namespace3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";

            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace(primary);
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace(secondary);
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace(tertiary);

            var strategy = new ReplicatedNamespacePartitioningStrategy(settings);

            Assert.AreEqual(3, strategy.GetNamespaces("endpoint1", PartitioningIntent.Creating).Count());
        }

        [Test]
        public void Replicated_partitioning_strategy_will_return_all_connectionstrings_for_purpose_of_receiving()
        {
            const string primary = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            const string secondary = "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            const string tertiary = "Endpoint=sb://namespace3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";

            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace(primary);
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace(secondary);
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace(tertiary);

            var strategy = new ReplicatedNamespacePartitioningStrategy(settings);

            Assert.AreEqual(3, strategy.GetNamespaces("endpoint1", PartitioningIntent.Receiving).Count());
        }

        [Test]
        public void Replicated_partitioning_strategy_will_throw_if_no_namespace_are_provided()
        {
            var settings = new SettingsHolder();

            Assert.Throws<ConfigurationErrorsException>(() => new ReplicatedNamespacePartitioningStrategy(settings));
        }

        [Test]
        public void Replicated_partitioning_strategy_will_throw_if_too_little_namespaces_are_provided()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace("Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey");

            Assert.Throws<ConfigurationErrorsException>(() => new ReplicatedNamespacePartitioningStrategy(settings));
        }

    }
}