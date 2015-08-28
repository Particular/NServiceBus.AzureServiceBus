namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using NServiceBus.Azure.WindowsAzureServiceBus.Tests;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_receiving_from_queues
    {
        [Test]
        public void Can_start_and_stop_notifier()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator();
            var namespaceManagerLifeCycleManager = new NamespaceManagerLifeCycleManager(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(namespaceManagerLifeCycleManager, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageReceiverCreator = new MessageReceiverCreator(messagingFactoryLifeCycleManager, settings);
            var clientEntityLifeCycleManager = new ClientEntityLifeCycleManager(messageReceiverCreator, settings);
            var creator = new AzureServiceBusQueueCreator(settings);
            var brokeredMessageConverter = new DefaultBrokeredMessagesToIncomingMessagesConverter();

            // create the queue
            var namespaceManager = namespaceManagerLifeCycleManager.Get(AzureServiceBusConnectionString.Value);
            creator.CreateAsync("myqueue", namespaceManager).Wait();

            var notifier = new MessageReceiverNotifier(clientEntityLifeCycleManager, brokeredMessageConverter, settings);

            notifier.Initialize("myqueue", AzureServiceBusConnectionString.Value, message => Task.FromResult(true), 10);

            notifier.Start().Wait();
            notifier.Stop().Wait();

            //cleanup 
            namespaceManager.DeleteQueueAsync("myqueue").Wait();
        }

        [Test]
        public void Can_start_stop_and_restart_notifier()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator();
            var namespaceManagerLifeCycleManager = new NamespaceManagerLifeCycleManager(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(namespaceManagerLifeCycleManager, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageReceiverCreator = new MessageReceiverCreator(messagingFactoryLifeCycleManager, settings);
            var clientEntityLifeCycleManager = new ClientEntityLifeCycleManager(messageReceiverCreator, settings);
            var creator = new AzureServiceBusQueueCreator(settings);
            var brokeredMessageConverter = new DefaultBrokeredMessagesToIncomingMessagesConverter();

            // create the queue
            var namespaceManager = namespaceManagerLifeCycleManager.Get(AzureServiceBusConnectionString.Value);
            creator.CreateAsync("myqueue", namespaceManager).Wait();

            var notifier = new MessageReceiverNotifier(clientEntityLifeCycleManager, brokeredMessageConverter, settings);

            notifier.Initialize("myqueue", AzureServiceBusConnectionString.Value, message => Task.FromResult(true), 10);

            notifier.Start().Wait();
            notifier.Stop().Wait();

            notifier.Start().Wait();
            notifier.Stop().Wait();

            //cleanup 
            namespaceManager.DeleteQueueAsync("myqueue").Wait();
        }
    }
}