namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus.Messaging;
    using Azure.WindowsAzureServiceBus.Tests;
    using NServiceBus.Azure.Transports.WindowsAzureServiceBus;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_comparing_performance_between_built_in_batching_and_manual
    {
        [Test]
        public async Task Can_send_brokered_messages_fast()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Connectivity()
                .NumberOfClientsPerEntity(5)
                .MessagingFactories()
                .NumberOfMessagingFactoriesPerNamespace(5)
                .BatchFlushInterval(TimeSpan.FromMilliseconds(100));

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator();
            var namespaceManagerLifeCycleManager = new NamespaceManagerLifeCycleManager(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(namespaceManagerLifeCycleManager, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageSenderCreator = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            var entityLifecycleManager = new MessageSenderLifeCycleManager(messageSenderCreator, settings);
            var creator = new AzureServiceBusQueueCreator(settings);

            // create the queue
            var namespaceManager = namespaceManagerLifeCycleManager.Get(AzureServiceBusConnectionString.Value);
            await creator.CreateAsync("myqueue", namespaceManager);

            // perform the test

            var sw = new Stopwatch();

            sw.Start();
            var counter = 0;
            var tasks = new List<Task>();
            for (var j = 0; j < 10; j++)
            {
                for (var i = 0; i < 1000; i++)
                {
                    var sender = entityLifecycleManager.Get("myqueue", null, AzureServiceBusConnectionString.Value);
                    tasks.Add(sender.RetryOnThrottleAsync(s => s.SendAsync(new BrokeredMessage()), s => s.SendAsync(new BrokeredMessage()), TimeSpan.FromSeconds(10), 5));

                    counter++;
                }
            }

            //validate
            await Task.WhenAll(tasks);
            sw.Stop();
            Console.WriteLine("Sending {1} messages took {0} milliseconds", sw.ElapsedMilliseconds, counter);
            var faulted = tasks.Count(task => task.IsFaulted);
            Console.WriteLine("Total of {0} msgs / second", (((double)(10000 - faulted)) / sw.ElapsedMilliseconds) * 1000);

            Assert.IsTrue(sw.ElapsedMilliseconds < TimeSpan.FromMinutes(1).TotalMilliseconds);

            //cleanup 
            await namespaceManager.DeleteQueueAsync("myqueue");
        }

        [Test]
        public async Task Can_send_batches_of_brokered_messages_fast()
        {
            // default settings
            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var extensions = new TransportExtensions<AzureServiceBusTransport>(settings);

            extensions.Connectivity()
                .NumberOfClientsPerEntity(5)
                .MessagingFactories()
                .NumberOfMessagingFactoriesPerNamespace(5)
                .BatchFlushInterval(TimeSpan.FromMilliseconds(0)); // turns of native batching

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator();
            var namespaceManagerLifeCycleManager = new NamespaceManagerLifeCycleManager(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(namespaceManagerLifeCycleManager, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageSenderCreator = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            var entityLifecycleManager = new MessageSenderLifeCycleManager(messageSenderCreator, settings);
            var creator = new AzureServiceBusQueueCreator(settings);

            // create the queue
            var namespaceManager = namespaceManagerLifeCycleManager.Get(AzureServiceBusConnectionString.Value);
            await creator.CreateAsync("myqueue", namespaceManager);

            // perform the test

            var sw = new Stopwatch();

            sw.Start();

            var counter = 0;

            var tasks = new List<Task>();

            for (var j = 0; j < 10; j++)
            {
                var batch = new List<BrokeredMessage>();
                for (var i = 0; i < 1000; i++)
                {
                    batch.Add(new BrokeredMessage());
                    counter++;
                }
                var sender = entityLifecycleManager.Get("myqueue", null, AzureServiceBusConnectionString.Value);
                tasks.Add(sender.RetryOnThrottleAsync(s => s.SendBatchAsync(batch), s => s.SendBatchAsync(batch.CloneWithMessageId()), TimeSpan.FromSeconds(10), 5));
            }
            await Task.WhenAll(tasks);

            //validate
            sw.Stop();
            Console.WriteLine("Sending {1} messages took {0} milliseconds", sw.ElapsedMilliseconds, counter);
            var faulted = tasks.Count(task => task.IsFaulted);
            Console.WriteLine("Total of {0} msgs / second", (((double)(10000 - faulted)) / sw.ElapsedMilliseconds) * 1000);
            Assert.IsTrue(sw.ElapsedMilliseconds < TimeSpan.FromMinutes(1).TotalMilliseconds);

            //cleanup 
            await namespaceManager.DeleteQueueAsync("myqueue");
        }
    }
}