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
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
            namespacesDefinition.Add("namespace", AzureServiceBusConnectionString.Value, NamespacePurpose.Partitioning);

            settings.Set(WellKnownConfigurationKeys.Connectivity.MessageReceivers.PrefetchCount, 500);
            settings.Set(WellKnownConfigurationKeys.Connectivity.MessageReceivers.ReceiveMode, ReceiveMode.PeekLock);
            settings.Set(WellKnownConfigurationKeys.Connectivity.SendViaReceiveQueue, true);

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator(settings);
            var namespaceManagerLifeCycleManager = new NamespaceManagerLifeCycleManager(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(namespaceManagerLifeCycleManager, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageReceiverCreator = new MessageReceiverCreator(messagingFactoryLifeCycleManager, settings);
            var clientEntityLifeCycleManager = new MessageReceiverLifeCycleManager(messageReceiverCreator, settings);
            var creator = new AzureServiceBusQueueCreator(settings);

            var brokeredMessageConverter = new DefaultBrokeredMessagesToIncomingMessagesConverter(settings, new PassThroughMapper());

            // create the queue
            var namespaceManager = namespaceManagerLifeCycleManager.Get("namespace");
            var queue = await creator.Create("myqueue", namespaceManager);

            var receivedMessages = 0;
            var completed = new AsyncAutoResetEvent(false);

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

            var notifier = new MessageReceiverNotifier(clientEntityLifeCycleManager, brokeredMessageConverter, settings);
            notifier.Initialize(new EntityInfo { Path = "myqueue", Namespace = new RuntimeNamespaceInfo("namespace", AzureServiceBusConnectionString.Value) },
                (message, context) =>
                {
                    Interlocked.Increment(ref receivedMessages);
                    if (receivedMessages == expected)
                    {
                        completed.Set();
                    }
                    return TaskEx.Completed;
                }, null, null, 10);


            var sw = new Stopwatch();
            sw.Start();
            notifier.Start();
            await completed.WaitAsync(cts.Token).IgnoreCancellation();
            sw.Stop();

            await notifier.Stop();

            Assert.IsTrue(receivedMessages == expected);
            Console.WriteLine($"Receiving {receivedMessages} messages took {sw.ElapsedMilliseconds} milliseconds");
            Console.WriteLine("Total of {0} msgs / second", (double)receivedMessages / sw.ElapsedMilliseconds * 1000);

            // make sure messages are autocompleted
            Assert.That(queue.MessageCount, Is.EqualTo(0), "Messages where not completed!");

            //cleanup
            await namespaceManager.DeleteQueue("myqueue");
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