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

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_receiving_incomingmessages_from_queues
    {
        [Test]
        public async Task Can_start_and_stop_notifier()
        {
            // default settings
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            settings.Set(new TopologySettings());
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
            namespacesDefinition.Add("namespace", AzureServiceBusConnectionString.Value, NamespacePurpose.Partitioning);

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
            await creator.Create("myqueue", namespaceManager);

            // perform the test
            var notifier = new MessageReceiverNotifier(messageReceiverCreator, brokeredMessageConverter, BuildMessageReceiverNotifierSettings(settings));

            notifier.Initialize(new EntityInfoInternal { Path =  "myqueue", Namespace = new RuntimeNamespaceInfo("namespace", AzureServiceBusConnectionString.Value)}, (message, context) => TaskEx.Completed, null, null, null, 10);

            notifier.Start();
            await notifier.Stop();

            //cleanup
            await namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public async Task Can_start_stop_and_restart_notifier()
        {
            // default settings
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            settings.Set(new TopologySettings());
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
            namespacesDefinition.Add("namespace", AzureServiceBusConnectionString.Value, NamespacePurpose.Partitioning);

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
            await creator.Create("myqueue", namespaceManager);

            // perform the test
            var notifier = new MessageReceiverNotifier(messageReceiverCreator, brokeredMessageConverter, BuildMessageReceiverNotifierSettings(settings));

            notifier.Initialize(new EntityInfoInternal { Path = "myqueue", Namespace = new RuntimeNamespaceInfo("namespace", AzureServiceBusConnectionString.Value) }, (message, context) => TaskEx.Completed, null, null, null, 10);

            notifier.Start();
            await notifier.Stop();

            notifier.Start();
            await notifier.Stop();

            //cleanup
            await namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public async Task Can_receive_an_incoming_message()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            // default settings
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            settings.Set(new TopologySettings());
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
            namespacesDefinition.Add("namespace", AzureServiceBusConnectionString.Value, NamespacePurpose.Partitioning);

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator(settings);
            var namespaceLifecycleManager = new NamespaceManagerLifeCycleManagerInternal(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(namespaceLifecycleManager, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageReceiverCreator = new MessageReceiverCreator(messagingFactoryLifeCycleManager, settings);
            var messageSenderCreator = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            var creator = new AzureServiceBusQueueCreator(settings.Get<TopologySettings>().QueueSettings, settings);

            var brokeredMessageConverter = new BrokeredMessagesToIncomingMessagesConverter(settings, new PassThroughMapper(settings));

            // create the queue
            var namespaceManager = namespaceLifecycleManager.Get("namespace");
            await creator.Create("myqueue", namespaceManager);

            // put a message on the queue
            var sender = await messageSenderCreator.Create("myqueue", null, "namespace");
            await sender.Send(new BrokeredMessage());

            // perform the test
            var notifier = new MessageReceiverNotifier(messageReceiverCreator, brokeredMessageConverter, BuildMessageReceiverNotifierSettings(settings));

            var completed = new AsyncManualResetEvent(false);
            var error = new AsyncManualResetEvent(false);

            Exception ex = null;
            var received = false;

            notifier.Initialize(new EntityInfoInternal { Path = "myqueue", Namespace = new RuntimeNamespaceInfo("namespace", AzureServiceBusConnectionString.Value) }, (message, context) =>
            {
                received = true;

                completed.Set();

                return TaskEx.Completed;
            },
            exception =>
            {
                ex = exception;

                error.Set();

                return TaskEx.Completed;
            }, null, null, 1);

            notifier.Start();

            await Task.WhenAny(completed.WaitAsync(cts.Token).IgnoreCancellation(), error.WaitAsync(cts.Token).IgnoreCancellation());

            // validate
            Assert.IsTrue(received);
            Assert.IsNull(ex);

            //cleanup
            await notifier.Stop();
            await namespaceManager.DeleteQueue("myqueue");
        }

        [Test]
        public async Task Triggers_critical_error_when_receiver_cannot_be_started()
        {
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));
            // default settings
            var settings = DefaultConfigurationValues.Apply(SettingsHolderFactory.BuildWithSerializer());
            settings.Set(new TopologySettings());
            var namespacesDefinition = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
            namespacesDefinition.Add("namespace", AzureServiceBusConnectionString.Value, NamespacePurpose.Partitioning);

            // setup the infrastructure
            var namespaceManagerCreator = new NamespaceManagerCreator(settings);
            var namespaceLifecycleManager = new NamespaceManagerLifeCycleManagerInternal(namespaceManagerCreator);
            var messagingFactoryCreator = new MessagingFactoryCreator(namespaceLifecycleManager, settings);
            var messagingFactoryLifeCycleManager = new MessagingFactoryLifeCycleManager(messagingFactoryCreator, settings);
            var messageReceiverCreator = new MessageReceiverCreator(messagingFactoryLifeCycleManager, settings);

            var brokeredMessageConverter = new BrokeredMessagesToIncomingMessagesConverter(settings, new PassThroughMapper(settings));

            // perform the test
            var notifier = new MessageReceiverNotifier(messageReceiverCreator, brokeredMessageConverter, BuildMessageReceiverNotifierSettings(settings));

            var completed = new AsyncManualResetEvent(false);
            var error = new AsyncManualResetEvent(false);

            Exception ex = null;
            var received = false;

            notifier.Initialize(new EntityInfoInternal { Path = "myqueue", Namespace = new RuntimeNamespaceInfo("namespace", AzureServiceBusConnectionString.Value) }, (message, context) =>
                {
                    received = true;

                    completed.Set();

                    return TaskEx.Completed;
                },
                null,
                (message, exception) =>
                {
                    ex = exception;

                    error.Set();
                }, null, 1);

            notifier.Start();

            await Task.WhenAny(completed.WaitAsync(cts.Token).IgnoreCancellation(), error.WaitAsync(cts.Token).IgnoreCancellation());

            // validate
            Assert.IsFalse(received);
            Assert.IsNotNull(ex);
        }

        static MessageReceiverNotifierSettings BuildMessageReceiverNotifierSettings(SettingsHolder settings)
        {
            // default values set by DefaultConfigurationValues.Apply - shouldn't hardcode those here, so OK to use settings
            return new MessageReceiverNotifierSettings(
                ReceiveMode.PeekLock,
                settings.HasExplicitValue<TransportTransactionMode>() ? settings.Get<TransportTransactionMode>() : settings.SupportedTransactionMode(),
                settings.Get<TimeSpan>(WellKnownConfigurationKeys.Connectivity.MessageReceivers.AutoRenewTimeout),
                settings.Get<int>(WellKnownConfigurationKeys.Connectivity.NumberOfClientsPerEntity));
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