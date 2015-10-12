namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Azure.WindowsAzureServiceBus.Tests;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_receiving_brokeredmessages_from_queues
    {
        [Test]
        public async Task Can_receive_a_brokered_message()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator();
            var namespaceManagerLifeCycleManager = new NamespaceManagerLifeCycleManager(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(namespaceManagerLifeCycleManager, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageReceiverCreator = new MessageReceiverCreator(messagingFactoryLifeCycleManager, settings);
            var messageSenderCreator = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            var creator = new AzureServiceBusQueueCreator(settings);

            // create the queue
            var namespaceManager = namespaceManagerLifeCycleManager.Get(AzureServiceBusConnectionString.Value);
            await creator.CreateAsync("myqueue", namespaceManager);

            // put a message on the queue
            var sender = await messageSenderCreator.CreateAsync("myqueue", null, AzureServiceBusConnectionString.Value);
            await sender.SendAsync(new BrokeredMessage());

            // perform the test
            var receiver = await messageReceiverCreator.CreateAsync("myqueue", AzureServiceBusConnectionString.Value);

            var completed = new AsyncAutoResetEvent(false);
            var error = new AsyncAutoResetEvent(false);

            Exception ex = null;
            var received = false;

            var options = new OnMessageOptions();
            options.ExceptionReceived += (o, args) =>
            {
                ex = args.Exception;

                error.Set();
            };
            receiver.OnMessageAsync(message =>
            {
                received = true;

                completed.Set();

                return Task.FromResult(true);
            }, options);

            await Task.WhenAny(completed.WaitOne(), error.WaitOne());

            // validate
            Assert.IsTrue(received);
            Assert.IsNull(ex);

            //cleanup 
            await receiver.CloseAsync();
            await namespaceManager.DeleteQueueAsync("myqueue");
        }
    }
}