namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Routing
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using AcceptanceTesting.Customization;
    using AzureServiceBus;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using Configuration.AdvanceExtensibility;
    using Settings;
    using NUnit.Framework;

    public class When_scaling_out_senders_that_uses_callbacks : NServiceBusAcceptanceTest
    {
        const int numberOfMessagesToSend = 5;

        [Test]
        public async Task Should_only_deliver_response_to_one_of_the_instances()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<ServerThatRespondsToCallbacks>()
                .WithEndpoint<ScaledOutClient>(b =>
                {
                    b.CustomConfig(c =>
                    {
                        c.MakeInstanceUniquelyAddressable("A");
                        c.GetSettings().Set("Client", "A");
                    });
                    b.When(async (bus, c) =>
                    {
                        for (var i = 0; i < numberOfMessagesToSend; i++)
                        {
                            var sendOptions = new SendOptions();
                            sendOptions.RouteReplyToThisInstance();
                            await bus.Send(new MyRequest()
                            {
                                Client = "A"
                            }, sendOptions);
                        }
                    });
                })
                .WithEndpoint<ScaledOutClient>(b =>
                {
                    b.CustomConfig(c =>
                    {
                        c.MakeInstanceUniquelyAddressable("B");
                        c.GetSettings().Set("Client", "B");
                    });
                    b.When(async (bus, c) =>
                    {
                        for (var i = 0; i < numberOfMessagesToSend; i++)
                        {
                            var sendOptions = new SendOptions();
                            sendOptions.RouteReplyToThisInstance();
                            await bus.Send(new MyRequest()
                            {
                                Client = "B"
                            }, sendOptions);
                        }
                    });
                })
                .Done(c => c.RepliesReceived >= numberOfMessagesToSend * 2)
                .Run();

            Assert.AreEqual(2 * numberOfMessagesToSend, context.RepliesReceived);
        }

        public class ScaledOutClient : EndpointConfigurationBuilder
        {
            public ScaledOutClient()
            {
                EndpointSetup<DefaultServer>(endpointConfiguration =>
                    endpointConfiguration.ConfigureTransport().Routing().RouteToEndpoint(typeof(MyRequest), typeof(ServerThatRespondsToCallbacks)));
            }

            class MyResponseHandler : IHandleMessages<MyResponse>
            {
                public ReadOnlySettings Settings { get; set; }

                public Context Context { get; set; }

                public Task Handle(MyResponse message, IMessageHandlerContext context)
                {
                    if (Settings.Get<string>("Client") != message.Client)
                    {
                        throw new Exception("Wrong endpoint got the response.");
                    }
                    Context.ReplyReceived();
                    return TaskEx.Completed;
                }
            }
        }

        public class ServerThatRespondsToCallbacks : EndpointConfigurationBuilder
        {
            public ServerThatRespondsToCallbacks()
            {
                EndpointSetup<DefaultServer>();
            }

            class MyRequestHandler : IHandleMessages<MyRequest>
            {
                public Task Handle(MyRequest message, IMessageHandlerContext context)
                {
                    return context.Reply(new MyResponse()
                    {
                        Client = message.Client
                    });
                }
            }
        }

        class MyRequest : IMessage
        {
            public string Client { get; set; }
        }

        class MyResponse : IMessage
        {
            public string Client { get; set; }
        }

        class Context : ScenarioContext
        {
            int repliesReceived;

            public int RepliesReceived => repliesReceived;

            public void ReplyReceived()
            {
                Interlocked.Increment(ref repliesReceived);
            }
        }
    }
}