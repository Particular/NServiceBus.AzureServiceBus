namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.TransportEncoding
{
    using System.Threading.Tasks;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NServiceBus.AzureServiceBus;
    using NUnit.Framework;

    public class When_sending_to_an_endpoint_with_different_transport_encoding : NServiceBusAcceptanceTest
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
                    .Done(c => c.WasCalled)
                    .Run();

            Assert.True(context.WasCalled, "The message handler should be called");
            Assert.AreEqual(context.OriginalMessageId, context.ReceivedMessageId, "The message handler should be called");
        }

        public class Context : ScenarioContext
        {
            public bool WasCalled { get; set; }
            public string OriginalMessageId { get; set; }
            public string ReceivedMessageId { get; set; }
        }

        public class Sender : EndpointConfigurationBuilder
        {
            public Sender()
            {
                EndpointSetup<DefaultServer>(busConfiguration =>
                {
                    busConfiguration.UseTransport<AzureServiceBusTransport>().BrokeredMessageBodyType(SupportedBrokeredMessageBodyTypes.Stream);
                }).AddMapping<MyMessage>(typeof(Receiver));
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
                    TestContext.ReceivedMessageId = message.Id;
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
