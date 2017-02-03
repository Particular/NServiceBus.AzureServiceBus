namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Receiving
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AzureServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using TestUtils;
    using Transport.AzureServiceBus;
    using Settings;
    using NUnit.Framework;

#pragma warning disable 618
    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_receiving_brokeredmessages_from_queues
    {
        [Test]
        public async Task Can_receive_a_brokered_message()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
            namespacesDefinition.Add("namespace", AzureServiceBusConnectionString.Value, NamespacePurpose.Partitioning);

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator(settings);
            var namespaceManagerLifeCycleManager = new NamespaceManagerLifeCycleManager(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(namespaceManagerLifeCycleManager, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageReceiverCreator = new MessageReceiverCreator(messagingFactoryLifeCycleManager, settings);
            var messageSenderCreator = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            var creator = new AzureServiceBusQueueCreator(settings);

            // create the queue
            var namespaceManager = namespaceManagerLifeCycleManager.Get("namespace");
            await creator.Create("myqueue", namespaceManager);

            // put a message on the queue
            var sender = await messageSenderCreator.Create("myqueue", null, "namespace");
            await sender.Send(new BrokeredMessage());

            // perform the test
            var receiver = await messageReceiverCreator.Create("myqueue", "namespace");

            var completed = new AsyncManualResetEvent(false);
            var error = new AsyncManualResetEvent(false);

            Exception ex = null;
            var received = false;

            var options = new OnMessageOptions();
            options.ExceptionReceived += (o, args) =>
            {
                ex = args.Exception;

                error.Set();
            };
            receiver.OnMessage(message =>
            {
                received = true;

                completed.Set();

                return TaskEx.Completed;
            }, options);

            await Task.WhenAny(completed.WaitAsync(cts.Token).IgnoreCancellation(), error.WaitAsync(cts.Token).IgnoreCancellation());

            // validate
            Assert.IsTrue(received);
            Assert.IsNull(ex);

            //cleanup
            await receiver.CloseAsync();
            await namespaceManager.DeleteQueue("myqueue");
        }
    }
}