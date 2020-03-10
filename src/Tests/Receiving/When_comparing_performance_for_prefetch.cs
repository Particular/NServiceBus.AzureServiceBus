namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Receiving
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
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
    public class When_comparing_performance_for_prefetch
    {
        [Test]
        public async Task Can_receive_messages_with_prefetch_fast()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            // default settings
            var settings = DefaultConfigurationValues.Apply(new SettingsHolder());
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
            namespacesDefinition.Add("namespace", AzureServiceBusConnectionString.Value, NamespacePurpose.Partitioning);

            settings.Set(WellKnownConfigurationKeys.Connectivity.MessageReceivers.PrefetchCount, 500);
            settings.Set(WellKnownConfigurationKeys.Connectivity.MessageReceivers.ReceiveMode, ReceiveMode.PeekLock);
            settings.Set(WellKnownConfigurationKeys.Connectivity.SendViaReceiveQueue, true);

            // default values set by DefaultConfigurationValues.Apply - shouldn't hardcode those here, so OK to use settings
            var messageReceiverNotifierSettings = new MessageReceiverNotifierSettings(
                ReceiveMode.PeekLock,
                settings.HasExplicitValue<TransportTransactionMode>() ? settings.Get<TransportTransactionMode>() : settings.SupportedTransactionMode(),
                settings.Get<TimeSpan>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.AutoRenewTimeout),
                settings.Get<int>(WellKnownConfigurationKeys.Connectivity.NumberOfClientsPerEntity));

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator(settings);
            var namespaceLifeCycleManager = new NamespaceManagerLifeCycleManagerInternal(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(namespaceLifeCycleManager, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageReceiverCreator = new MessageReceiverCreator(messagingFactoryLifeCycleManager, settings);
            var creator = new AzureServiceBusQueueCreator(new TopologyQueueSettings(), settings);

            var brokeredMessageConverter = new BrokeredMessagesToIncomingMessagesConverter(settings, new PassThroughMapper(settings));

            // create the queue
            var namespaceManager = namespaceLifeCycleManager.Get("namespace");
            var queue = await creator.Create("myqueue", namespaceManager);

            var receivedMessages = 0;
            var completed = new AsyncManualResetEvent(false);

            // sending messages to the queue
            var senderFactory = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            var sender = await senderFactory.Create("myqueue", null, "namespace");
            var counter = 0;
            var tasks = new List<Task>();
            for (var j = 0; j < 10; j++)
            {
                var batch = new List<BrokeredMessage>();
                for (var i = 0; i < 100; i++)
                {
                    batch.Add(new BrokeredMessage(Encoding.UTF8.GetBytes("Whatever" + counter)));
                    counter++;
                }
                tasks.Add(sender.RetryOnThrottleAsync(s => s.SendBatch(batch), s => s.SendBatch(batch.Select(x => x.Clone())), TimeSpan.FromSeconds(10), 5));
            }
            await Task.WhenAll(tasks);
            var faulted = tasks.Count(task => task.IsFaulted);
            var expected = 1000 - faulted;
            // sending messages to the queue is done

            var notifier = new MessageReceiverNotifier(messageReceiverCreator, brokeredMessageConverter, messageReceiverNotifierSettings);
            notifier.Initialize(new EntityInfoInternal { Path = "myqueue", Namespace = new RuntimeNamespaceInfo("namespace", AzureServiceBusConnectionString.Value) },
                (message, context) =>
                {
                    var numberOfMessages = Interlocked.Increment(ref receivedMessages);
                    if (numberOfMessages == expected)
                    {
                        completed.Set();
                    }
                    return TaskEx.Completed;
                }, null, null, null, 32);


            var sw = new Stopwatch();
            sw.Start();
            notifier.Start();
            await completed.WaitAsync(cts.Token).IgnoreCancellation();
            sw.Stop();

            await notifier.Stop();

            Assert.IsTrue(receivedMessages == expected);
            Console.WriteLine($"Receiving {receivedMessages} messages took {sw.ElapsedMilliseconds} milliseconds");
            Console.WriteLine("Total of {0} msgs / second", (double)receivedMessages / sw.ElapsedMilliseconds * 1000);

            // make sure messages are auto-completed
            Assert.That(queue.MessageCount, Is.EqualTo(0), "Messages where not completed!");

            //cleanup
            await namespaceManager.DeleteQueue("myqueue");
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