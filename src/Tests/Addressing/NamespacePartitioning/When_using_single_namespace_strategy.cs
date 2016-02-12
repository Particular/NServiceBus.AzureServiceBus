namespace NServiceBus.AzureServiceBus.Tests
{
    using System.Configuration;
    using System.Linq;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_single_namespace_strategy
    {
        [Test]
        public void Single_partitioning_strategy_will_return_a_single_connectionstring_for_the_purpose_of_creating()
        {
            const string connectionstring = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";

            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace("name", connectionstring);

            var strategy = new SingleNamespacePartitioningStrategy(settings);

            Assert.AreEqual(1, strategy.GetNamespaces("endpoint1", PartitioningIntent.Creating).Count());
        }

        [Test]
        public void Single_partitioning_strategy_will_return_a_single_connectionstring_for_the_purpose_of_receiving()
        {
            const string connectionstring = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";

            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace("name", connectionstring);

            var strategy = new SingleNamespacePartitioningStrategy(settings);

            Assert.AreEqual(1, strategy.GetNamespaces("endpoint1", PartitioningIntent.Receiving).Count());
        }

        [Test]
        public void Single_partitioning_strategy_will_return_a_single_connectionstring_for_the_purpose_of_sending()
        {
            const string connectionstring = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";

            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace("name", connectionstring);

            var strategy = new SingleNamespacePartitioningStrategy(settings);

            Assert.AreEqual(1, strategy.GetNamespaces("endpoint1", PartitioningIntent.Sending).Count());
        }

        [Test]
        public void Single_partitioning_strategy_will_return_configured_namespace_for_any_endpoint_name()
        {
            const string connectionstring = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";

            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace("name", connectionstring);

            var strategy = new SingleNamespacePartitioningStrategy(settings);

            Assert.AreEqual(new NamespaceInfo(connectionstring, NamespaceMode.Active), strategy.GetNamespaces("endpoint1", PartitioningIntent.Creating).First());
        }

        [Test]
        public void Single_partitioning_strategy_will_throw_if_no_namespace_defined()
        {
            var settings = new SettingsHolder();
            
            Assert.Throws<ConfigurationErrorsException>(() => new SingleNamespacePartitioningStrategy(settings));
        }

        [Test]
        public void Single_partitioning_strategy_will_throw_if_more_namespaces_defined()
        {
            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace("name1", "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey");
            extensions.UseDefaultTopology().Addressing().NamespacePartitioning().AddNamespace("name2", "Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey");

            Assert.Throws<ConfigurationErrorsException>(() => new SingleNamespacePartitioningStrategy(settings));
        }
    }

}