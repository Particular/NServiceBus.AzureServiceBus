namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using Microsoft.ServiceBus;
    using NServiceBus.Azure.WindowsAzureServiceBus.Tests;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_receiving_from_queues
    {
        [Test]
        public void Can_start_receiving()
        {
            //// default settings
            //var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            
            //// setup the infrastructure
            //var namespaceManagerCreator = new NamespaceManagerCreator();
            //var namespaceManagerLifeCycleManager = new NamespaceManagerLifeCycleManager(namespaceManagerCreator);
            //var messagingFactoryCreator = new MessagingFactoryCreator(namespaceManagerLifeCycleManager, settings);
            //var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            //var messageReceiverCreator = new MessageReceiverCreator(messagingFactoryLifeCycleManager, settings);
            //var clientEntityLifeCycleManager = new ClientEntityLifeCycleManager(messageReceiverCreator, settings);
            //var creator = new AzureServiceBusQueueCreator(settings);
            //var brokeredMessageConverter = new DefaultBrokeredMessagesToIncomingMessagesConverter();

            //// create the queue
            //var namespaceManager = namespaceManagerLifeCycleManager.Get(AzureServiceBusConnectionString.Value);
            //creator.CreateAsync("myqueue", namespaceManager).Wait();

            //var notifier = new MessageReceiverNotifier(clientEntityLifeCycleManager, brokeredMessageConverter, settings);

        

            ////cleanup 
            //namespaceManager.DeleteQueueAsync("myqueue").Wait();
        }
    }
}