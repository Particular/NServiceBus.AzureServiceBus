namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Sending
{
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using TestUtils;
    using AzureServiceBus;
    using Settings;
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
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
            namespacesDefinition.Add("namespace", AzureServiceBusConnectionString.Value, NamespacePurpose.Partitioning);

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator(settings);
            var namespaceManagerLifeCycleManager = new NamespaceManagerLifeCycleManager(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(namespaceManagerLifeCycleManager, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageSenderCreator = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            var creator = new AzureServiceBusQueueCreator(settings);

            // create the queue
            var namespaceManager = namespaceManagerLifeCycleManager.Get("namespace");
            await creator.Create("myqueue", namespaceManager);

            // perform the test
            var sender = await messageSenderCreator.Create("myqueue", null, "namespace");
            await sender.Send(new BrokeredMessage());

            //validate
            var queue = await namespaceManager.GetQueue("myqueue");
            Assert.IsTrue(queue.MessageCount > 0);
            
            //cleanup 
            await namespaceManager.DeleteQueue("myqueue");
        }
    }
}