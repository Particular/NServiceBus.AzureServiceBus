namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.TransportEncoding
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using AcceptanceTesting.Customization;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using Transport.AzureServiceBus;
    using NUnit.Framework;

    public class When_sending_to_an_endpoint_with_different_transport_encoding : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_receive_the_message()
        {
            var context = await Scenario.Define<Context>()
                    .WithEndpoint<EndpointA>(b => b.When((bus, ctx) =>
                    {
                        ctx.OriginalMessageIdIdentifier = "MyMessageId" + Guid.NewGuid().ToString("N");
                        var sendOptions = new SendOptions();
                        sendOptions.SetMessageId(ctx.OriginalMessageIdIdentifier);

                        return bus.Send(new MyMessage { Identifier = ctx.OriginalMessageIdIdentifier }, sendOptions);
                    }))
                    .WithEndpoint<EndpointB>()
                    .Done(c => c.EndpointBReceivedMessage && c.EndpointAReceivedReply)
                    .Run();

            Assert.True(context.EndpointBReceivedMessage, "The message should have been handled, but it wasn't.");
            Assert.True(context.EndpointAReceivedReply, "Reply message should have been received, but it wasn't.");
        }

        public class Context : ScenarioContext
        {
            public bool EndpointBReceivedMessage { get; set; }
            public bool EndpointAReceivedReply { get; set; }
            public string OriginalMessageIdIdentifier { get; set; }
        }

        public class EndpointA : EndpointConfigurationBuilder
        {
            public EndpointA()
            {
                EndpointSetup<DefaultServer>(endpointConfiguration =>
                {
                    endpointConfiguration.UseTransport<AzureServiceBusTransport>().BrokeredMessageBodyType(SupportedBrokeredMessageBodyTypes.Stream);
                    endpointConfiguration.ConfigureTransport().Routing().RouteToEndpoint(typeof(MyMessage), typeof(EndpointB));
                });
            }

            public class MyReplyHandler : IHandleMessages<MyReply>
            {
                public Context TestContext { get; set; }

                public Task Handle(MyReply message, IMessageHandlerContext context)
                {
                    if (TestContext.OriginalMessageIdIdentifier != message.Identifier)
                    {
                        return Task.FromResult(0);
                    }

                    TestContext.EndpointAReceivedReply = true;
                    return context.Reply(new MyReply { Identifier = message.Identifier });
                }
            }
        }

        public class EndpointB : EndpointConfigurationBuilder
        {
            public EndpointB()
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
                    if (TestContext.OriginalMessageIdIdentifier != message.Identifier)
                    {
                        return Task.FromResult(0);
                    }

                    TestContext.EndpointBReceivedMessage = true;
                    return context.Reply(new MyReply { Identifier = message.Identifier });
                }
            }
        }

        public class MyMessage : IMessage
        {
            public string Identifier { get; set; }
        }
        public class MyReply : IMessage
        {
            public string Identifier { get; set; }
        }
    }
}
