namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using Azure.WindowsAzureServiceBus.Tests;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_operating_BasicTopology
    {
        [Test]
        public async Task Receives_incoming_messages_from_endpoint_queue()
        {
            // setting up the environment
            var container = new FuncBuilder();

            var settings = new SettingsHolder();
            container.Register(typeof(SettingsHolder), () => settings);
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            settings.SetDefault<EndpointName>(new EndpointName("sales"));
            extensions.Topology().Addressing().NamespacePartitioning().AddNamespace(AzureServiceBusConnectionString.Value);

            var topology = new BasicTopology(settings, container);

            topology.InitializeSettings();
            topology.InitializeContainer();

            // create the topology
            var topologyCreator = (ICreateTopology)container.Build(typeof(TopologyCreator));
            await topologyCreator.Create(topology.Determine(Purpose.Creating));

            // setup the operator
            var topologyOperator = (IOperateTopology)container.Build(typeof(TopologyOperator));

            var completed = new AsyncAutoResetEvent(false);
            var error = new AsyncAutoResetEvent(false);

            var received = false;
            Exception ex = null;

            topologyOperator.OnIncomingMessage((message, context) =>
            {
                received = true;

                completed.Set();

                return Task.FromResult(true);
            });
            topologyOperator.OnError(exception =>
            {
                ex = exception;

                error.Set();

                return Task.FromResult(true);
            });

            // execute
            await topologyOperator.Start(topology.Determine(Purpose.Receiving), 1);

            // send message to queue
            var senderFactory = (MessageSenderCreator)container.Build(typeof(MessageSenderCreator));
            var sender = (IMessageSender) await senderFactory.CreateAsync("sales", AzureServiceBusConnectionString.Value);
            await sender.SendAsync(new BrokeredMessage());

            await Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(received);
            Assert.IsNull(ex);

            // cleanup 
            await topologyOperator.Stop();

            var namespaceLifeCycle = (IManageNamespaceManagerLifeCycle)container.Build(typeof(IManageNamespaceManagerLifeCycle));
            var namespaceManager = namespaceLifeCycle.Get(AzureServiceBusConnectionString.Value);
            await namespaceManager.DeleteQueueAsync("sales");
        }
    }
}
