namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Seam
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AzureServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using Tests;
    using TestUtils;
    using Transport.AzureServiceBus;
    using Settings;
    using Transport;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_receiving_messages
    {
        [Test]
        public async Task Pushes_received_message_into_pipeline()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

            await TestUtility.Delete("sales");

            var settings = new SettingsHolder();
            settings.Set("NServiceBus.SharedQueue", "sales");
            new DefaultConfigurationValues().Apply(settings);

            // setting up the environment
            var container = new TransportPartsContainer();

            var topologySectionManagerInternal = await SetupEndpointOrientedTopology(container, settings, "sales");

            // setup the operator
            var clientEntities = new MessageReceiverLifeCycleManager(new MessageReceiverCreator(new MessagingFactoryLifeCycleManager(new MessagingFactoryCreator(new NamespaceManagerLifeCycleManagerInternal(new NamespaceManagerCreator(settings)), settings), settings), settings), settings);
            var converter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings, new DefaultConnectionStringToNamespaceAliasMapper(settings));

            var pump = new MessagePump(new TopologyOperator(clientEntities, converter, settings), clientEntities, converter, topologySectionManagerInternal, settings);

            var completed = new AsyncManualResetEvent(false);
            //var error = new AsyncManualResetEvent(false);

            var received = false;
            Exception ex = null;

            // Dummy CriticalError
            var criticalError = new CriticalError(ctx => TaskEx.Completed);

            await pump.Init(context =>
            {
                received = true;

                completed.Set();

                return TaskEx.Completed;

            }, null, criticalError, new PushSettings("sales", "error", false, TransportTransactionMode.ReceiveOnly));

            // execute
            pump.Start(new PushRuntimeSettings(1));

            // send message to queue
            var senderFactory = new MessageSenderCreator(new MessagingFactoryLifeCycleManager(new MessagingFactoryCreator(new NamespaceManagerLifeCycleManagerInternal(new NamespaceManagerCreator(settings)), settings), settings), settings);
            var sender = await senderFactory.Create("sales", null, "namespaceName");
            await sender.Send(new BrokeredMessage());

            await completed.WaitAsync(cts.Token).IgnoreCancellation(); // Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(received);
            Assert.IsNull(ex);

            // cleanup
            await pump.Stop();
        }

        async Task<ITopologySectionManagerInternal> SetupEndpointOrientedTopology(TransportPartsContainer container, SettingsHolder settings, string enpointname)
        {
            settings.Set<Conventions>(new Conventions());
            var topology = new EndpointOrientedTopologyInternal(container);

            topology.Initialize(settings);

            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            settings.SetDefault("NServiceBus.Routing.EndpointName", enpointname);
            extensions.NamespacePartitioning().AddNamespace("namespaceName", AzureServiceBusConnectionString.Value);

            // create the topologySectionManager
            var topologyCreator = (ICreateTopologyInternal)container.Resolve(typeof(TopologyCreator));
            var sectionManager = container.Resolve<ITopologySectionManagerInternal>();
            await topologyCreator.Create(sectionManager.DetermineResourcesToCreate(new QueueBindings()));
            container.RegisterSingleton<TopologyOperator>();
            return sectionManager;
        }
    }
}