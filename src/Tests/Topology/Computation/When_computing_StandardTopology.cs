namespace NServiceBus.AzureServiceBus.Tests
{
    using System.Linq;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_computing_StandardTopology
    {
        [Test]
        public void Determines_the_namespace_from_partitioning_strategy()
        {
            var container = new FuncContainer();

            var settings = new SettingsHolder();
            container.Register(typeof(SettingsHolder), () => settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            settings.SetDefault<EndpointName>(new EndpointName("sales"));
            var connectionstring = "Endpoint=sb://namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(connectionstring);

            var topology = new StandardTopology();

            topology.InitializeSettings(settings);
            topology.InitializeContainer(new FuncBuilder(container));
            topology.UseBuilder(new FuncBuilder(container));
            var definition = topology.DetermineResourcesToCreate();

            var namespaceInfo = new NamespaceInfo(connectionstring, NamespaceMode.Active);
            Assert.IsTrue(definition.Namespaces.Any(nsi => nsi == namespaceInfo));
        }

        [Test]
        public void Determines_there_should_be_a_queue_with_same_name_as_endpointname()
        {
            var container = new FuncContainer();

            var settings = new SettingsHolder();
            container.Register(typeof(SettingsHolder), () => settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            settings.SetDefault<EndpointName>(new EndpointName("sales"));
            var connectionstring = "Endpoint=sb://namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(connectionstring);

            var topology = new StandardTopology();

            topology.InitializeSettings(settings);
            topology.InitializeContainer(new FuncBuilder(container));
            topology.UseBuilder(new FuncBuilder(container));
            var definition = topology.DetermineResourcesToCreate();

            Assert.AreEqual(1, definition.Entities.Count(ei => ei.Path == "sales" && ei.Type == EntityType.Queue && ei.Namespace.ConnectionString == connectionstring));
        }

        [Test]
        public void Determines_there_should_be_a_topic_with_same_name_as_endpointname_followed_by_dot_events()
        {
            var container = new FuncContainer();

            var settings = new SettingsHolder();
            container.Register(typeof(SettingsHolder), () => settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            settings.SetDefault<EndpointName>(new EndpointName("sales"));
            var connectionstring = "Endpoint=sb://namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(connectionstring);

            var topology = new StandardTopology();

            topology.InitializeSettings(settings);
            topology.InitializeContainer(new FuncBuilder(container));
            topology.UseBuilder(new FuncBuilder(container));
            var definition = topology.DetermineResourcesToCreate();

            Assert.AreEqual(1, definition.Entities.Count(ei => ei.Path == "sales.events" && ei.Type == EntityType.Topic && ei.Namespace.ConnectionString == connectionstring ));
        }
    }
}
