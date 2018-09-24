namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Receiving
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Transactions;
    using AzureServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NUnit.Framework;
    using Settings;
    using TestUtils;
    using Transport;
    using Transport.AzureServiceBus;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_completion_fails_after_processing_failure
    {
        [Test]
        public async Task Should_have_rolled_back_message_to_error_queue()
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
                TransportTransactionMode.SendsAtomicWithReceive,
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
            await creator.Create("completionfailure", namespaceManager);

            var completed = new AsyncManualResetEvent(false);

            // sending messages to the queue
            var senderFactory = new MessageSenderCreator(messagingFactoryLifeCycleManager, settings);
            var sender = await senderFactory.Create("completionfailure", null, "namespace");
            var messageToSend = new BrokeredMessage(Encoding.UTF8.GetBytes("Whatever"))
            {
                MessageId = Guid.NewGuid().ToString()
            };
            await sender.Send(messageToSend);
            // sending messages to the queue is done

            var rolledBack = new RollbackDetection();

            var notifier = new MessageReceiverNotifier(messageReceiverCreator, brokeredMessageConverter, messageReceiverNotifierSettings);
            notifier.Initialize(new EntityInfoInternal { Path = "completionfailure", Namespace = new RuntimeNamespaceInfo("namespace", AzureServiceBusConnectionString.Value) },
                (message, context) =>
                {
                    if (message.MessageId == messageToSend.MessageId)
                    {
                        Transaction.Current.EnlistVolatile(new EmulateCompletionFailure(), EnlistmentOptions.EnlistDuringPrepareRequired);
                        throw new Exception("Force processing failure");
                    }
                    return TaskEx.Completed;
                }, null, null, context =>
                {
                    if (context.Message.MessageId == messageToSend.MessageId)
                    {
                        Transaction.Current.EnlistVolatile(rolledBack, EnlistmentOptions.None);
                        completed.Set();
                    }
                    return Task.FromResult(ErrorHandleResult.Handled);
                }, 1);


            var sw = new Stopwatch();
            sw.Start();
            notifier.Start();
            await completed.WaitAsync(cts.Token).IgnoreCancellation();
            sw.Stop();

            await notifier.Stop();

            Assert.IsTrue(rolledBack.RolledBack, "Should have rolled back the error message.");
            Console.WriteLine($"Callback processing took {sw.ElapsedMilliseconds} milliseconds");

            //cleanup
            await namespaceManager.DeleteQueue("completionfailure");
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

        public class EmulateCompletionFailure : IEnlistmentNotification
        {
            public void Prepare(PreparingEnlistment preparingEnlistment)
            {
                preparingEnlistment.ForceRollback();
            }

            public void Commit(Enlistment enlistment)
            {
               enlistment.Done();
            }

            public void Rollback(Enlistment enlistment)
            {
               enlistment.Done();
            }

            public void InDoubt(Enlistment enlistment)
            {
                enlistment.Done();
            }
        }

        public class RollbackDetection : IEnlistmentNotification
        {
            public bool RolledBack { get; set; }

            public void Prepare(PreparingEnlistment preparingEnlistment)
            {
                preparingEnlistment.Prepared();
            }

            public void Commit(Enlistment enlistment)
            {
                enlistment.Done();
            }

            public void Rollback(Enlistment enlistment)
            {
                RolledBack = true;
                enlistment.Done();
            }

            public void InDoubt(Enlistment enlistment)
            {
                enlistment.Done();
            }
        }
    }
}