namespace NServiceBus.AzureServiceBus.Tests
{
    using System.Configuration;
    using System.Linq;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_roundrobin_strategy_on_multiple_namespaces
    {
        private static readonly string Primary = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
        private static readonly string Secondary = "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
        private static readonly string Tertiary = "Endpoint=sb://namespace3.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";

        private RoundRobinNamespacePartitioningStrategy _strategy;

        [SetUp]
        public void SetUp()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace("namespace1", Primary);
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace("namespace2", Secondary);
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace("namespace3", Tertiary);

            _strategy = new RoundRobinNamespacePartitioningStrategy(settings);
        }

        [Test]
        public void Roundrobin_partitioning_strategy_will_return_a_single_connectionstring_for_purpose_of_sending()
        {
            var namespaces = _strategy.GetNamespaces("endpoint1", PartitioningIntent.Sending);

            Assert.AreEqual(1, namespaces.Count());
        }

        [Test]
        public void Roundrobin_partitioning_strategy_will_return_all_connectionstrings_for_purpose_of_creating()
        {
            var namespaces = _strategy.GetNamespaces("endpoint1", PartitioningIntent.Creating);

            Assert.AreEqual(3, namespaces.Count());
        }

        [Test]
        public void Roundrobin_partitioning_strategy_will_return_all_connectionstrings_for_purpose_of_receiving()
        {
            var namespaces = _strategy.GetNamespaces("endpoint1", PartitioningIntent.Receiving);

            Assert.AreEqual(3, namespaces.Count());
        }

        [Test]
        public void Roundrobin_partitioning_strategy_will_circle_active_namespace()
        {
            Assert.AreEqual(new NamespaceInfo(Primary, NamespaceMode.Active), _strategy.GetNamespaces("endpoint1", PartitioningIntent.Sending).First());
            Assert.AreEqual(new NamespaceInfo(Secondary, NamespaceMode.Active), _strategy.GetNamespaces("endpoint1", PartitioningIntent.Sending).First());
            Assert.AreEqual(new NamespaceInfo(Tertiary, NamespaceMode.Active), _strategy.GetNamespaces("endpoint1", PartitioningIntent.Sending).First());

            Assert.AreEqual(new NamespaceInfo(Primary, NamespaceMode.Active), _strategy.GetNamespaces("endpoint1", PartitioningIntent.Sending).First());
            Assert.AreEqual(new NamespaceInfo(Secondary, NamespaceMode.Active), _strategy.GetNamespaces("endpoint1", PartitioningIntent.Sending).First());
            Assert.AreEqual(new NamespaceInfo(Tertiary, NamespaceMode.Active), _strategy.GetNamespaces("endpoint1", PartitioningIntent.Sending).First());
        }
        
        [Test]
        public void Roundrobin_partitioning_strategy_will_throw_if_no_namespace_are_provided()
        {
            var settings = new SettingsHolder();

            Assert.Throws<ConfigurationErrorsException>(() => new RoundRobinNamespacePartitioningStrategy(settings));
        }

        [Test]
        public void Roundrobin_partitioning_strategy_will_throw_if_too_little_namespaces_are_provided()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace("namespace1", Primary);
            
            Assert.Throws<ConfigurationErrorsException>(() => new RoundRobinNamespacePartitioningStrategy(settings));
        }

    }
}