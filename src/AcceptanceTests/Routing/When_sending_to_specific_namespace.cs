namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Routing
{
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using AcceptanceTesting.Customization;
    using AzureServiceBus;
    using AzureServiceBus.AcceptanceTests.Infrastructure;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;

    public class When_using_multiple_namespaces : NServiceBusAcceptanceTest
    {
        static string connectionString = connectionString = TestUtility.DefaultConnectionString;
        static string targetConnectionString = TestUtility.FallbackConnectionString;

        [Test]
        public async Task Should_support_request_reply_across_namespaces_using_aliases()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<EndpointInSourceNamespace>(b =>
                {
                    b.CustomConfig(c =>
                        {
                            var transport = c.ConfigureAzureServiceBus();
                            transport.UseNamespaceAliasesInsteadOfConnectionStrings();
                            transport.NamespacePartitioning().AddNamespace("source", connectionString);
                            transport.NamespaceRouting().AddNamespace("target", targetConnectionString);
                        })
                    .When(async (bus, c) =>
                    {
                        var sendOptions = new SendOptions();
                        sendOptions.SetDestination("usingmultiplenamespaces.endpointintargetnamespace@target");
                        await bus.Send(new MyRequest(), sendOptions);
                    });
                })
                .WithEndpoint<EndpointInTargetNamespace>(b => b.CustomConfig(c =>
                {
                    var transport = c.ConfigureAzureServiceBus();
                    transport.UseNamespaceAliasesInsteadOfConnectionStrings();
                    transport.NamespaceRouting().AddNamespace("source", connectionString);
                    transport.NamespacePartitioning().AddNamespace("target", targetConnectionString);
                }))
                .Done(c => c.ReplyReceived)
                .Run();

            Assert.IsTrue(context.RequestReceived, "context.RequestReceived");
            Assert.IsTrue(context.ReplyReceived, "context.ReplyReceived");
        }


        [Test]
        public async Task Should_support_request_reply_across_namespaces_using_connection_strings()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<EndpointInSourceNamespace>(b =>
                {
                    b.CustomConfig(c =>
                    {
                        var transport = c.ConfigureAzureServiceBus();
                        transport.NamespacePartitioning().AddNamespace("source", connectionString);
                        transport.NamespaceRouting().AddNamespace("target", targetConnectionString);
                    })
                    .When(async (bus, c) =>
                    {
                        var sendOptions = new SendOptions();
                        sendOptions.SetDestination("usingmultiplenamespaces.endpointintargetnamespace@" + targetConnectionString);
                        await bus.Send(new MyRequest(), sendOptions);
                    });
                })
                .WithEndpoint<EndpointInTargetNamespace>(b => b.CustomConfig(c =>
                {
                    var transport = c.ConfigureAzureServiceBus();
                    transport.NamespaceRouting().AddNamespace("source", connectionString);
                    transport.NamespacePartitioning().AddNamespace("target", targetConnectionString);
                }))
                .Done(c => c.ReplyReceived)
                .Run();

            Assert.IsTrue(context.RequestReceived, "context.RequestReceived");
            Assert.IsTrue(context.ReplyReceived, "context.ReplyReceived");
        }

        [Test]
        public async Task Should_support_request_reply_across_namespaces_using_contained_endpoint()
        {
            var context = await Scenario.Define<Context>()
                 .WithEndpoint<EndpointInSourceNamespace>(b =>
                 {
                     b.CustomConfig(c =>
                     {
                         var transport = c.ConfigureAzureServiceBus();
                         transport.NamespacePartitioning().AddNamespace("source", connectionString);
                         var targetNamespace = transport.NamespaceRouting().AddNamespace("target", targetConnectionString);
                         targetNamespace.RegisteredEndpoints.Add(Conventions.EndpointNamingConvention(typeof(EndpointInTargetNamespace)));
                     })
                     .When((bus, c) => bus.Send(new MyRequest()));
                 })
                 .WithEndpoint<EndpointInTargetNamespace>(b => b.CustomConfig(c =>
                 {
                     var transport = c.ConfigureAzureServiceBus();
                     transport.NamespaceRouting().AddNamespace("source", connectionString);
                     transport.NamespacePartitioning().AddNamespace("target", targetConnectionString);
                 }))
                 .Done(c => c.ReplyReceived)
                 .Run();

            Assert.IsTrue(context.RequestReceived, "context.RequestReceived");
            Assert.IsTrue(context.ReplyReceived, "context.ReplyReceived");
        }

        public class EndpointInSourceNamespace : EndpointConfigurationBuilder
        {
            public EndpointInSourceNamespace()
            {
                EndpointSetup<DefaultServer>(endpointConfiguration =>
                        endpointConfiguration.ConfigureTransport().Routing().RouteToEndpoint(typeof(MyRequest), typeof(EndpointInTargetNamespace)));
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

        public class EndpointInTargetNamespace : EndpointConfigurationBuilder
        {
            public EndpointInTargetNamespace()
            {
                EndpointSetup<DefaultServer>();
            }

            class MyRequestHandler : IHandleMessages<MyRequest>
            {
                public Context Context { get; set; }

                public Task Handle(MyRequest message, IMessageHandlerContext context)
                {
                    Context.RequestReceived = true;
                    return context.Reply(new MyResponse());
                }
            }
        }

        public class MyRequest : IMessage
        {
        }

        public class MyResponse : IMessage
        {
        }

        class Context : ScenarioContext
        {
            public bool RequestReceived { get; set; }
            public bool ReplyReceived { get; set; }
        }
    }
}