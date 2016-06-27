namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.TransportEncoding
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using AcceptanceTesting;
    using AcceptanceTesting.Customization;
    using AcceptanceTesting.Support;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using Transport.AzureServiceBus;
    using MessageMutator;
    using NUnit.Framework;

    public class When_receiving_a_message_with_unknown_transport_encoding : NServiceBusAcceptanceTest
    {
        TimeSpan testExecutionTimeout = TimeSpan.FromMinutes(2);

        [Test]
        public async Task Should_deadletter_message()
        {
            var context = new Context();

            var scenarioTask = Scenario.Define<Context>(contextInitializer => context = contextInitializer)
                    .WithEndpoint<Sender>(b => b.When((bus, ctx) =>
                    {
                        ctx.OriginalMessageId = "MyMessageId";
                        var sendOptions = new SendOptions();
                        sendOptions.SetMessageId(ctx.OriginalMessageId);

                        return bus.Send(new MyMessage { Id = ctx.OriginalMessageId }, sendOptions);
                    }))
                    .WithEndpoint<Receiver>()
                    .Done(ctx => ctx.MessageWasMovedToDlq)
                    .Run(new RunSettings { TestExecutionTimeout = testExecutionTimeout });

            // can't apply a message spy pattern here since message will be always poisonous
            var rawReceiveTask = Task.Run(async () =>
            {
                // delay raw receive from the DLQ to allow endpoint (queue) creation
                await Task.Delay(TimeSpan.FromSeconds(10));

                var connectionString = Environment.GetEnvironmentVariable("AzureServiceBusTransport.ConnectionString");
                var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(connectionString));
                var factory = MessagingFactory.CreateAsync(namespaceManager.Address, namespaceManager.Settings.TokenProvider).GetAwaiter().GetResult();
                var dlqPath = Conventions.EndpointNamingConvention(typeof(Receiver)) + "/$DeadLetterQueue";
                var receiver = await factory.CreateMessageReceiverAsync(dlqPath, ReceiveMode.ReceiveAndDelete).ConfigureAwait(false);
                var message = await receiver.ReceiveAsync(testExecutionTimeout).ConfigureAwait(false);
                var receivedMessageIdMatchesTheOriginal = message.Properties["NServiceBus.MessageId"].ToString() == context.OriginalMessageId;
                var testRunIdMatchesTheCurrectTestRun = message.Properties["$AcceptanceTesting.TestRunId"].ToString() == context.TestRunId.ToString();
                var deliveredOnceOnly = message.DeliveryCount == 1;
                context.MessageWasMovedToDlq = receivedMessageIdMatchesTheOriginal && testRunIdMatchesTheCurrectTestRun && deliveredOnceOnly;
            });

            await Task.WhenAll(scenarioTask, rawReceiveTask).ConfigureAwait(false);

            Assert.False(context.WasCalled, "The message handler should not be called");
            Assert.True(context.MessageWasMovedToDlq, "Message should be in DLQ");
        }

        public class Context : ScenarioContext
        {
            public bool WasCalled { get; set; }
            public string OriginalMessageId { get; set; }
            public bool MessageWasMovedToDlq { get; set; }
        }

        public class Sender : EndpointConfigurationBuilder
        {
            public Sender()
            {
                EndpointSetup<DefaultServer>(busConfiguration =>
                {
                    busConfiguration.UseTransport<AzureServiceBusTransport>().BrokeredMessageBodyType(SupportedBrokeredMessageBodyTypes.Stream);
                    busConfiguration.RegisterComponents(components => components.ConfigureComponent<SetTransportEncodingToUnknownMutator>(DependencyLifecycle.InstancePerCall));
                }).AddMapping<MyMessage>(typeof(Receiver));
            }

            class SetTransportEncodingToUnknownMutator : IMutateOutgoingTransportMessages
            {
                public Task MutateOutgoing(MutateOutgoingTransportMessageContext context)
                {
                    context.OutgoingHeaders["NServiceBus.Transport.Encoding"] = "unknown";
                    return Task.FromResult(0);
                }
            }
        }

        public class Receiver : EndpointConfigurationBuilder
        {
            public Receiver()
            {
                EndpointSetup<DefaultServer>(busConfiguration =>
                {
                    busConfiguration.UseTransport<AzureServiceBusTransport>().BrokeredMessageBodyType(SupportedBrokeredMessageBodyTypes.ByteArray);
                });
            }

            public class MyMessageHandler : IHandleMessages<MyMessage>
            {
                public Context TestContext { get; set; }

                public Task Handle(MyMessage message, IMessageHandlerContext context)
                {
                    TestContext.WasCalled = true;

                    return Task.FromResult(0);
                }
            }
        }

        public class MyMessage : IMessage
        {
            public string Id { get; set; }
        }
    }
}
