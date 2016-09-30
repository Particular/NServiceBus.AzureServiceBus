namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Receiving
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AzureServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using TestUtils;
    using Transport.AzureServiceBus;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_incoming_message_processing_takes_longer_than_LockDuration
    {
        [Test]
        public async Task AutoRenewTimout_will_extend_lock_for_processing_to_finish()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
            namespacesDefinition.Add("namespace", AzureServiceBusConnectionString.Value, NamespacePurpose.Partitioning);

            // set lock duration on a queue to 20 seconds and emulate message processing that takes longer than that, but less than AutoRenewTimeout
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Queues.LockDuration, TimeSpan.FromSeconds(20));

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator(settings);
            var namespaceManagerLifeCycleManager = new NamespaceManagerLifeCycleManager(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(namespaceManagerLifeCycleManager, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageReceiverCreator = new MessageReceiverCreator(messagingFactoryCreator, settings);
            var clientEntityLifeCycleManager = new MessageReceiverLifeCycleManager(messageReceiverCreator, settings);
            var creator = new AzureServiceBusQueueCreator(settings);

            var brokeredMessageConverter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings, new PassThroughMapper());

            // create the queue
            var namespaceManager = namespaceManagerLifeCycleManager.Get("namespace");
            await creator.Create("autorenewtimeout", namespaceManager);

            var receivedMessages = 0;
            var completed = new AsyncAutoResetEvent(false);

            // sending messages to the queue
            var senderFactory = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            var sender = await senderFactory.Create("autorenewtimeout", null, "namespace");
            var messageToSend = new BrokeredMessage(Encoding.UTF8.GetBytes("Whatever"))
            {
                MessageId = Guid.NewGuid().ToString()
            };
            await sender.Send(messageToSend);
            // sending messages to the queue is done

            var notifier = new MessageReceiverNotifier(clientEntityLifeCycleManager, brokeredMessageConverter, settings);
            notifier.Initialize(new EntityInfo { Path = "autorenewtimeout", Namespace = new RuntimeNamespaceInfo("namespace", AzureServiceBusConnectionString.Value) },
                async (message, context) =>
                {
                    if (message.MessageId == messageToSend.MessageId)
                    {
                        Interlocked.Increment(ref receivedMessages);
                        if (receivedMessages > 1)
                        {
                            Assert.Fail("Callback should only receive one message once, but it did more than that.");
                        }
                        await Task.Delay(TimeSpan.FromSeconds(30));
                        completed.Set();
                    }
                }, null, null, 10);


            var sw = new Stopwatch();
            sw.Start();
            notifier.Start();
            await completed.WaitAsync(cts.Token).IgnoreCancellation();
            await Task.Delay(TimeSpan.FromSeconds(10));
            sw.Stop();

            await notifier.Stop();

            Assert.AreEqual(1, receivedMessages, $"Expected to receive message once, but got {receivedMessages}.");
            Console.WriteLine($"Callback processing took {sw.ElapsedMilliseconds} milliseconds");

            //cleanup
            await namespaceManager.DeleteQueue("autorenewtimeout");
        }

        class PassThroughMapper : ICanMapConnectionStringToNamespaceAlias
        {
            public EntityAddress Map(EntityAddress value)
            {
                return value;
            }
        }
    }
}