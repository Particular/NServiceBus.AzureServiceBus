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
    public class When_using_single_namespace_strategy
    {
        static string ConnectionString = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
        static string OtherConnectionString = "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";

        static string Name = "namespace1";
        static string OtherName = "namespace2";

        SingleNamespacePartitioning strategy;

        [SetUp]
        public void SetUp()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.AddNamespace(Name, ConnectionString);

            strategy = new SingleNamespacePartitioning(settings);
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

            Assert.AreEqual(new RuntimeNamespaceInfo(Name, ConnectionString), namespaces.First());
        }

        [Test]
        public void Single_partitioning_strategy_will_throw_if_no_namespace_defined()
        {
            var settings = new SettingsHolder();

            Assert.Throws<ConfigurationErrorsException>(() => new SingleNamespacePartitioning(settings));
        }

        [Test]
        public void Single_partitioning_strategy_will_throw_if_more_namespaces_defined()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.AddNamespace(Name, ConnectionString);
            extensions.AddNamespace(OtherName, OtherConnectionString);

            Assert.Throws<ConfigurationErrorsException>(() => new SingleNamespacePartitioning(settings));
        }
    }

}