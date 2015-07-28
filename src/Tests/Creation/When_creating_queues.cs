namespace NServiceBus.AzureServiceBus.Tests
{
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_creating_queues
    {
        [Test]
        public void Does_not_create_queues_when_createqueues_is_set_to_false()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var namespaceManager = NamespaceManager.CreateFromConnectionString("Endpoint=sb://servicebus-unittesting.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=y15MuGqqMKL67kUrKPdKiq+kPBrhW+774NiDVXsjSDU=");

            settings.Set("Transport.CreateQueues", false);
            
            var creator = new AzureServiceBusQueueCreator(settings);

            creator.Create("myqueue", namespaceManager);

            Assert.IsFalse(namespaceManager.QueueExists("myqueue"));
        }

        [Test]
        public void Uses_queue_description_when_provided_by_user()
        {
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);
            var namespaceManager = NamespaceManager.CreateFromConnectionString("Endpoint=sb://servicebus-unittesting.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=y15MuGqqMKL67kUrKPdKiq+kPBrhW+774NiDVXsjSDU=");

            var descriptionToUse = new QueueDescription("myqueue");

            settings.Set("Transport.CreateQueues", true);
            extensions.Topology().Resources().Queues().DescriptionFactory((name, s) => descriptionToUse);

            var creator = new AzureServiceBusQueueCreator(settings);

            var description = creator.Create("myqueue", namespaceManager);

            Assert.IsTrue(namespaceManager.QueueExists("myqueue"));
            Assert.AreEqual(descriptionToUse, description);

            //cleanup 
            namespaceManager.DeleteQueue("myqueue");

        }

    }

}