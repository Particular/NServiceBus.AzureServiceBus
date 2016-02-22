namespace NServiceBus.AzureServiceBus.Tests
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Azure.WindowsAzureServiceBus.Tests;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_sending_brokeredmessages_to_queues
    {
        [Test]
        public async Task Can_send_a_brokered_message()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var namespacesDefinition = settings.Get<NamespacesDefinition>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);
            namespacesDefinition.AddDefault(AzureServiceBusConnectionString.Value);

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator(settings);
            var namespaceManagerLifeCycleManager = new NamespaceManagerLifeCycleManager(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(namespaceManagerLifeCycleManager, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageSenderCreator = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            var creator = new AzureServiceBusQueueCreator(settings);

            // create the queue
            var namespaceManager = namespaceManagerLifeCycleManager.Get("default");
            await creator.Create("myqueue", namespaceManager);

            // perform the test
            var sender = await messageSenderCreator.Create("myqueue", null, "default");
            await sender.Send(new BrokeredMessage());

            //validate
            var queue = await namespaceManager.GetQueue("myqueue");
            Assert.IsTrue(queue.MessageCount > 0);
            
            //cleanup 
            await namespaceManager.DeleteQueue("myqueue");
        }
    }
}