namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_computing_topology_where_each_endpoint_has_a_queue_and_topic
    {
        [Test]
        public async Task Determines_the_namespace_from_partitioning_strategy()
        {
            var container = new FuncBuilder();

            var settings = new SettingsHolder();
            container.Register(typeof(SettingsHolder), () => settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            settings.SetDefault("EndpointName", "sales");
            var connectionstring = "Endpoint=sb://namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(connectionstring);

            var topology = new EachEndpointHasQueueAndTopic(settings, container);

            topology.InitializeSettings();
            topology.InitializeContainer();
            topology.Determine();

            Assert.IsTrue(topology.Definition.Namespaces.Any(n => n.ConnectionString == connectionstring));
        }

        [Test]
        public async Task Determines_there_should_be_a_queue_with_same_name_as_endpointname()
        {
           throw new NotImplementedException();
        }

        [Test]
        public async Task Determines_there_should_be_a_topic_with_same_name_as_endpointname_followed_by_dot_events()
        {
            throw new NotImplementedException();
        }

    }
}
