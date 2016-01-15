namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.TransportEncoding
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.AcceptanceTesting.Support;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.MessageMutator;
    using NUnit.Framework;

    public class When_receiving_a_message_with_unknown_transport_encoding : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_receive_the_message()
        {
            var context = await Scenario.Define<Context>()
                    .WithEndpoint<Sender>(b => b.When((bus, ctx) =>
                    {
                        ctx.OriginalMessageId = "MyMessageId";
                        var sendOptions = new SendOptions();
                        sendOptions.SetMessageId(ctx.OriginalMessageId);

                        return bus.Send(new MyMessage { Id = ctx.OriginalMessageId }, sendOptions);
                    }))
                    .WithEndpoint<Receiver>()
                    .Done(ctx =>
                    {
                        var namespaceManager = new NamespaceManagerAdapter(NamespaceManager.CreateFromConnectionString(Environment.GetEnvironmentVariable("AzureServiceBusTransport.ConnectionString"))); // get connection string from test env
                        var factory = MessagingFactory.Create(namespaceManager.Address, namespaceManager.Settings.TokenProvider);
                        var queueClient = factory.CreateQueueClient("receivingamessagewithunknowntransportencoding.receiver/$DeadLetterQueue", ReceiveMode.ReceiveAndDelete); // queue name from the test name
                        var message = queueClient.Receive();
                        ctx.MessageWasMovedToDlq = message.MessageId == ctx.OriginalMessageId && (message.Properties["$AcceptanceTesting.TestRunId"] as string) == ctx.TestRunId.ToString();
                        return ctx.MessageWasMovedToDlq;
                    })
                    .Run(new RunSettings() {TestExecutionTimeout = TimeSpan.FromMinutes(2)});

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
                    // TODO: this is wrong. We should be able to configure serialization w/o going through topology
                    busConfiguration.UseTransport<AzureServiceBusTransport>().UseDefaultTopology().Serialization().BrokeredMessageBodyType(SupportedBrokeredMessageBodyTypes.Stream);
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
                    // TODO: this is wrong. We should be able to configure serialization w/o going through topology
                    busConfiguration.UseTransport<AzureServiceBusTransport>().UseDefaultTopology().Serialization().BrokeredMessageBodyType(SupportedBrokeredMessageBodyTypes.ByteArray);
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
