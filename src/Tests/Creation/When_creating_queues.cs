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

            
            extensions.Topology().Resources().Queues().DescriptionFactory((name, s) => descriptionToUse);

            var creator = new AzureServiceBusQueueCreator(settings);

            var description = creator.Create("myqueue", namespaceManager);

            Assert.IsTrue(namespaceManager.QueueExists("myqueue"));
            Assert.AreEqual(descriptionToUse, description);

            //cleanup 
            namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public void Properly_sets_ForwardTo_on_the_created_entity()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString("Endpoint=sb://servicebus-unittesting.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=y15MuGqqMKL67kUrKPdKiq+kPBrhW+774NiDVXsjSDU=");

            // forwarding queue needs to exist before you can use it as a forwarding target
            // needs to be created with different settings as it cannot forward to itself obviously
            var originalsettings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var originalcreator = new AzureServiceBusQueueCreator(originalsettings);
            originalcreator.Create("myotherqueue", namespaceManager);

            // actual test
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().ForwardTo("myotherqueue");

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.Create("myqueue", namespaceManager);

            var real = namespaceManager.GetQueue("myqueue");

            Assert.IsTrue(real.ForwardTo.EndsWith("myotherqueue"));

            //cleanup 
            namespaceManager.DeleteQueue("myqueue");
            namespaceManager.DeleteQueue("myotherqueue");
        }

        [Test]
        public void Properly_sets_ForwardTo_on_the_created_entity_that_qualifies_condition()
        {
            var namespaceManager = NamespaceManager.CreateFromConnectionString("Endpoint=sb://servicebus-unittesting.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=y15MuGqqMKL67kUrKPdKiq+kPBrhW+774NiDVXsjSDU=");
           
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Topology().Resources().Queues().ForwardTo(name => name == "myqueue", "myotherqueue");

            var creator = new AzureServiceBusQueueCreator(settings);

            creator.Create("myotherqueue", namespaceManager);
            creator.Create("myqueue", namespaceManager);

            var real = namespaceManager.GetQueue("myqueue");
            var forwardReal = namespaceManager.GetQueue("myotherqueue");

            Assert.IsTrue(real.ForwardTo.EndsWith("myotherqueue"));
            Assert.IsTrue(string.IsNullOrEmpty(forwardReal.ForwardTo));

            //cleanup 
            namespaceManager.DeleteQueue("myqueue");
            namespaceManager.DeleteQueue("myotherqueue");
        }

    }



}