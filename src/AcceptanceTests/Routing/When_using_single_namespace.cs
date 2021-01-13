namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Routing
{
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using AcceptanceTesting.Customization;
    using AzureServiceBus;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;

    public class When_using_single_namespace : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_append_namespace_alias_to_reply_address_when_using_aliases()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<SourceEndpoint>(b =>
                {
                    b.When(async (bus, c) =>
                    {
                        var sendOptions = new SendOptions();
                        sendOptions.SetDestination("usingsinglenamespace.targetendpoint");
                        await bus.Send(new MyRequest(), sendOptions);
                    });
                })
                .WithEndpoint<TargetEndpoint>()
                .Done(c => c.ReplyReceived)
                .Run();

            Assert.IsTrue(context.RequestReceived, "context.RequestReceived");
            Assert.IsTrue(context.ReplyReceived, "context.ReplyReceived");
            Assert.IsTrue(context.ReplyToContainsNamespaceName, "context.ReplyToContainsNamespaceName");
        }


        public class SourceEndpoint : EndpointConfigurationBuilder
        {
            public SourceEndpoint()
            {
                EndpointSetup<DefaultServer>(c =>
                {
                    var transport = c.ConfigureAzureServiceBus();
                    transport.UseNamespaceAliasesInsteadOfConnectionStrings();

                    c.ConfigureTransport().Routing().RouteToEndpoint(typeof(MyRequest), typeof(TargetEndpoint));
                });
            }

            class MyResponseHandler : IHandleMessages<MyResponse>
            {
                public Context Context { get; set; }

                public Task Handle(MyResponse message, IMessageHandlerContext context)
                {
                    Context.ReplyReceived = true;
                    return TaskEx.Completed;
                }
            }
        }

        public class TargetEndpoint : EndpointConfigurationBuilder
        {
            public TargetEndpoint()
            {
                EndpointSetup<DefaultServer>(c =>
                {
                    var transport = c.ConfigureAzureServiceBus();
                    transport.UseNamespaceAliasesInsteadOfConnectionStrings();
                });
            }

            class MyRequestHandler : IHandleMessages<MyRequest>
            {
                public Context Context { get; set; }

                public Task Handle(MyRequest message, IMessageHandlerContext context)
                {
                    Context.RequestReceived = true;
                    Context.ReplyToContainsNamespaceName = context.ReplyToAddress.Contains("@default");
                    return context.Reply(new MyResponse());
                }
            }
        }

        public class MyRequest : IMessage
        {
        }

        public class MyRequestImpl : MyRequest
        {
        }

        public class MyResponse : IMessage
        {
        }

        class Context : ScenarioContext
        {
            public bool RequestReceived { get; set; }
            public bool ReplyReceived { get; set; }
            public bool ReplyToContainsNamespaceName { get; set; }
        }
    }
}