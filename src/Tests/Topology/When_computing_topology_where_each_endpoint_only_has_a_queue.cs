namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using System.Linq;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_computing_topology_where_each_endpoint_only_has_a_queue
    {
        [Test]
        public void Determines_the_namespace_from_partitioning_strategy()
        {
            var container = new FuncBuilder();

            var settings = new SettingsHolder();
            container.Register(typeof(SettingsHolder), () => settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            settings.SetDefault<EndpointName>(new EndpointName("sales"));
            var connectionstring = "Endpoint=sb://namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(connectionstring);

            var topology = new EachEndpointHasQueueOnly(settings, container);

            topology.InitializeSettings();
            topology.InitializeContainer();
            var definition = topology.Determine(Purpose.Creating);

            var namespaceInfo = new NamespaceInfo(connectionstring, NamespaceMode.Active);
            Assert.IsTrue(definition.Namespaces.Any(nsi => nsi == namespaceInfo));
        }

        [Test]
        public void Determines_there_should_be_a_queue_with_same_name_as_endpointname()
        {
            var container = new FuncBuilder();

            var settings = new SettingsHolder();
            container.Register(typeof(SettingsHolder), () => settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            settings.SetDefault<EndpointName>(new EndpointName("sales"));
            var connectionstring = "Endpoint=sb://namespace.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=somesecretkey";
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(connectionstring);

            var topology = new EachEndpointHasQueueOnly(settings, container);

            topology.InitializeSettings();
            topology.InitializeContainer();
            var definition = topology.Determine(Purpose.Creating);

            Assert.AreEqual(1, definition.Entities.Count(ei => ei.Path == "sales" && ei.Type == EntityType.Queue && ei.Namespace.ConnectionString == connectionstring));
        }

        [Test]
        public void Does_not_support_subscribing()
        {
            var container = new FuncBuilder();

            var settings = new SettingsHolder();
            settings.SetDefault<EndpointName>(new EndpointName("sales"));
            container.Register(typeof(SettingsHolder), () => settings);
           
            var topology = new EachEndpointHasQueueOnly(settings, container);

            topology.InitializeSettings();
            topology.InitializeContainer();
            
            Assert.Throws<NotSupportedException>(() => topology.Subscribe(typeof(MyEvent)));
        }

        [Test]
        public void Does_not_support_unsubscribing()
        {
            var container = new FuncBuilder();

            var settings = new SettingsHolder();
            settings.SetDefault<EndpointName>(new EndpointName("sales"));
            container.Register(typeof(SettingsHolder), () => settings);

            var topology = new EachEndpointHasQueueOnly(settings, container);

            topology.InitializeSettings();
            topology.InitializeContainer();


            Assert.Throws<NotSupportedException>(() => topology.Unsubscribe(typeof(MyEvent)));
        }

        class MyEvent
        {
        }
    }
}
