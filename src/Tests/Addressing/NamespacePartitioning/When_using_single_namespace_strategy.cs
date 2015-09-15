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
        public void Single_partitioning_strategy_will_return_a_single_connectionstring()
        {
            const string connectionstring = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";

            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(connectionstring);

            var strategy = new SingleNamespacePartitioningStrategy(settings);

            Assert.AreEqual(1, strategy.GetNamespaceInfo("endpoint1").Count());
        }

        [Test]
        public void Single_partitioning_strategy_will_return_configured_namespace_for_any_endpoint_name()
        {
            const string connectionstring = "Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";

            var settings = new SettingsHolder();
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(connectionstring);

            var strategy = new SingleNamespacePartitioningStrategy(settings);

            Assert.AreEqual(new NamespaceInfo(connectionstring, NamespaceMode.Active), strategy.GetNamespaceInfo("endpoint1").First());
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
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace("Endpoint=sb://namespace1.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey");
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace("Endpoint=sb://namespace2.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey");

            Assert.Throws<ConfigurationErrorsException>(() => new SingleNamespacePartitioningStrategy(settings));
        }
    }

}