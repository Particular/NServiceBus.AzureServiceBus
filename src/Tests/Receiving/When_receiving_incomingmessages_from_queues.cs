namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Azure.WindowsAzureServiceBus.Tests;
    using NServiceBus.AzureServiceBus.Topology.MetaModel;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_receiving_incomingmessages_from_queues
    {
        [Test]
        public async Task Can_start_and_stop_notifier()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);
            namespacesDefinition.AddDefault(AzureServiceBusConnectionString.Value);

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator(settings);
            var namespaceManagerLifeCycleManager = new NamespaceManagerLifeCycleManager(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(namespaceManagerLifeCycleManager, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageReceiverCreator = new MessageReceiverCreator(messagingFactoryLifeCycleManager, settings);
            var clientEntityLifeCycleManager = new MessageReceiverLifeCycleManager(messageReceiverCreator, settings);
            var creator = new AzureServiceBusQueueCreator(settings);

            var brokeredMessageConverter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings, new PassThroughtMapper());

            // create the queue
            var namespaceManager = namespaceManagerLifeCycleManager.Get("default");
            await creator.Create("myqueue", namespaceManager);

            // perform the test
            var notifier = new MessageReceiverNotifier(clientEntityLifeCycleManager, brokeredMessageConverter, settings);

            notifier.Initialize(new EntityInfo { Path =  "myqueue", Namespace = new RuntimeNamespaceInfo("default", AzureServiceBusConnectionString.Value)}, (message, context) => Task.FromResult(true), null, 10);

            notifier.Start();
            await notifier.Stop();

            //cleanup 
            await namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public async Task Can_start_stop_and_restart_notifier()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);
            namespacesDefinition.AddDefault(AzureServiceBusConnectionString.Value);

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator(settings);
            var namespaceManagerLifeCycleManager = new NamespaceManagerLifeCycleManager(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(namespaceManagerLifeCycleManager, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageReceiverCreator = new MessageReceiverCreator(messagingFactoryLifeCycleManager, settings);
            var clientEntityLifeCycleManager = new MessageReceiverLifeCycleManager(messageReceiverCreator, settings);
            var creator = new AzureServiceBusQueueCreator(settings);

            var brokeredMessageConverter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings, new PassThroughtMapper());

            // create the queue
            var namespaceManager = namespaceManagerLifeCycleManager.Get("default");
            await creator.Create("myqueue", namespaceManager);

            // perform the test
            var notifier = new MessageReceiverNotifier(clientEntityLifeCycleManager, brokeredMessageConverter, settings);

            notifier.Initialize(new EntityInfo { Path = "myqueue", Namespace = new RuntimeNamespaceInfo("default", AzureServiceBusConnectionString.Value) }, (message, context) => Task.FromResult(true), null, 10);

            notifier.Start();
            await notifier.Stop();

            notifier.Start();
            await notifier.Stop();

            //cleanup 
            await namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public async Task Can_receive_an_incoming_message()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);
            namespacesDefinition.AddDefault(AzureServiceBusConnectionString.Value);

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator(settings);
            var namespaceManagerLifeCycleManager = new NamespaceManagerLifeCycleManager(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(namespaceManagerLifeCycleManager, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageReceiverCreator = new MessageReceiverCreator(messagingFactoryLifeCycleManager, settings);
            var messageSenderCreator = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            var clientEntityLifeCycleManager = new MessageReceiverLifeCycleManager(messageReceiverCreator, settings);
            var creator = new AzureServiceBusQueueCreator(settings);

            var brokeredMessageConverter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings, new PassThroughtMapper());

            // create the queue
            var namespaceManager = namespaceManagerLifeCycleManager.Get("default");
            await creator.Create("myqueue", namespaceManager);

            // put a message on the queue
            var sender = await messageSenderCreator.Create("myqueue", null, "default");
            await sender.Send(new BrokeredMessage());

            // perform the test
            var notifier = new MessageReceiverNotifier(clientEntityLifeCycleManager, brokeredMessageConverter, settings);

            var completed = new AsyncAutoResetEvent(false);
            var error = new AsyncAutoResetEvent(false);
            
            Exception ex = null;
            var received = false;

            notifier.Initialize(new EntityInfo { Path = "myqueue", Namespace = new RuntimeNamespaceInfo("default", AzureServiceBusConnectionString.Value) }, (message, context) =>
            {
                received = true;

                completed.Set();

                return Task.FromResult(true);
            },
            exception =>
            {
                ex = exception;

                error.Set();

                return Task.FromResult(true);
            }, 1);

            notifier.Start();

            await Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(received);
            Assert.IsNull(ex);

            //cleanup 
            await notifier.Stop();
            await namespaceManager.DeleteQueue("myqueue");
        }

        private class PassThroughtMapper : ICanMapConnectionStringToNamespaceName
        {
            public EntityAddress Map(EntityAddress value)
            {
                return value;
            }
        }
    }
}