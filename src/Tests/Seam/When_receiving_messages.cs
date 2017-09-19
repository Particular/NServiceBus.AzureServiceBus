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

            var settings = SettingsHolderFactory.BuildWithSerializer();
            settings.Set("NServiceBus.SharedQueue", "sales");
            DefaultConfigurationValues.Apply(settings);

            settings.Set<Conventions>(new Conventions());
            var topology = new EndpointOrientedTopologyInternal();

            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            settings.SetDefault("NServiceBus.Routing.EndpointName", "sales");
            extensions.NamespacePartitioning().AddNamespace("namespaceName", AzureServiceBusConnectionString.Value);

            topology.Initialize(settings);

            // create the topologySectionManager
            var namespaceManagerCreator = new NamespaceManagerCreator(settings);

            var namespaceLifecycleManager = new NamespaceManagerLifeCycleManagerInternal(namespaceManagerCreator);
            var topologyCreator = new TopologyCreator(new AzureServiceBusSubscriptionCreatorV6(topology.Settings.SubscriptionSettings, settings),
                new AzureServiceBusQueueCreator(topology.Settings.QueueSettings,settings),  new AzureServiceBusTopicCreator(topology.Settings.TopicSettings),
                namespaceLifecycleManager, settings);

            var topologySectionManager = topology.TopologySectionManager;
            await topologyCreator.Create(topologySectionManager.DetermineResourcesToCreate(new QueueBindings()));

            // setup the operator
            var messageFactoryCreator = new MessagingFactoryCreator(namespaceLifecycleManager, settings);
            var messageFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messageFactoryCreator, settings);
            var receiverCreator = new MessageReceiverCreator(messageFactoryLifeCycleManager, settings);
            var receiversLifeCycleManager = new MessageReceiverLifeCycleManager(receiverCreator, settings);
            var converter = new BrokeredMessagesToIncomingMessagesConverter(settings, new DefaultConnectionStringToNamespaceAliasMapper(settings));

            var pump = new MessagePump(new TopologyOperator(receiversLifeCycleManager, converter, settings), receiversLifeCycleManager, converter, topologySectionManager, settings);

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
            var senderFactory = new MessageSenderCreator(messageFactoryLifeCycleManager, settings);
            var sender = await senderFactory.Create("sales", null, "namespaceName");
            await sender.Send(new BrokeredMessage());

            await completed.WaitAsync(cts.Token).IgnoreCancellation(); // Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(received);
            Assert.IsNull(ex);

            // cleanup
            await pump.Stop();
        }
    }
}