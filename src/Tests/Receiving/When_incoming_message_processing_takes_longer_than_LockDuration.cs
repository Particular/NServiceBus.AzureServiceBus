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
        public async Task AutoRenewTimeout_will_extend_lock_for_processing_to_finish()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            // default settings
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            settings.Set(new TopologySettings());
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
            namespacesDefinition.Add("namespace", AzureServiceBusConnectionString.Value, NamespacePurpose.Partitioning);

            // set lock duration on a queue to 5 seconds and emulate message processing that takes longer than that, but less than AutoRenewTimeout
            settings.Get<TopologySettings>().QueueSettings.LockDuration = TimeSpan.FromSeconds(2);
            settings.Set(WellKnownConfigurationKeys.Connectivity.MessageReceivers.AutoRenewTimeout, TimeSpan.FromSeconds(5));

            // default values set by DefaultConfigurationValues.Apply - shouldn't hardcode those here, so OK to use settings
            var messageReceiverNotifierSettings = new MessageReceiverNotifierSettings(
                ReceiveMode.PeekLock,
                settings.HasExplicitValue<TransportTransactionMode>() ? settings.Get<TransportTransactionMode>() : settings.SupportedTransactionMode(),
                settings.Get<TimeSpan>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.AutoRenewTimeout),
                settings.Get<int>(WellKnownConfigurationKeys.Connectivity.NumberOfClientsPerEntity));

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator(settings);
            var namespaceLifecycleManager = new NamespaceManagerLifeCycleManagerInternal(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(namespaceLifecycleManager, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageReceiverCreator = new MessageReceiverCreator(messagingFactoryLifeCycleManager, settings);
            var creator = new AzureServiceBusQueueCreator(settings.Get<TopologySettings>().QueueSettings, settings);

            var brokeredMessageConverter = new BrokeredMessagesToIncomingMessagesConverter(settings, new PassThroughMapper(settings));

            // create the queue
            var namespaceManager = namespaceLifecycleManager.Get("namespace");
            await creator.Create("autorenewtimeout", namespaceManager);

            var receivedMessages = 0;
            var completed = new AsyncManualResetEvent(false);

            // sending messages to the queue
            var senderFactory = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            var sender = await senderFactory.Create("autorenewtimeout", null, "namespace");
            var messageToSend = new BrokeredMessage(Encoding.UTF8.GetBytes("Whatever"))
            {
                MessageId = Guid.NewGuid().ToString()
            };
            await sender.Send(messageToSend);
            
            // sending messages to the queue is done
            var notifier = new MessageReceiverNotifier(messageReceiverCreator, brokeredMessageConverter, messageReceiverNotifierSettings);
            notifier.Initialize(new EntityInfoInternal { Path = "autorenewtimeout", Namespace = new RuntimeNamespaceInfo("namespace", AzureServiceBusConnectionString.Value) },
                async (message, context) =>
                {
                    if (message.MessageId == messageToSend.MessageId)
                    {
                        Interlocked.Increment(ref receivedMessages);
                        if (receivedMessages > 1)
                        {
                            Assert.Fail("Callback should only receive one message once, but it did more than that.");
                        }
                        await Task.Delay(TimeSpan.FromSeconds(5), cts.Token);
                        completed.Set();
                    }
                }, null, null, null, 1);


            var sw = new Stopwatch();
            sw.Start();
            notifier.Start();
            await completed.WaitAsync(cts.Token).IgnoreCancellation();
            sw.Stop();

            await notifier.Stop();

            Assert.AreEqual(1, receivedMessages, $"Expected to receive message once, but got {receivedMessages}.");
            Console.WriteLine($"Callback processing took {sw.ElapsedMilliseconds} milliseconds");

            //cleanup
            await namespaceManager.DeleteQueue("autorenewtimeout");
        }

        class PassThroughMapper : DefaultConnectionStringToNamespaceAliasMapper
        {
            public PassThroughMapper(ReadOnlySettings settings) : base(settings)
            {
            }

            public override EntityAddress Map(EntityAddress value)
            {
                return value;
            }
        }
    }
}