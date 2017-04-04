namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Routing
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using AcceptanceTesting.Customization;
    using AcceptanceTesting.Support;
    using AzureServiceBus;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NServiceBus.AcceptanceTests.ScenarioDescriptors;
    using NUnit.Framework;

    public class When_using_multiple_namespaces : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_support_request_reply_across_namespaces_using_aliases()
        {
            var runSettings = new RunSettings();
            runSettings.TestExecutionTimeout = TimeSpan.FromMinutes(1);

            var ctx = new AzureServiceBusTransportConfigContext();
            ctx.Callback = (endpointName, extensions) =>
            {
                var connectionString = EnvironmentHelper.GetEnvironmentVariable("AzureServiceBusTransport.ConnectionString");
                var targetConnectionString = EnvironmentHelper.GetEnvironmentVariable("AzureServiceBus.ConnectionString.Fallback");
                extensions.UseNamespaceAliasesInsteadOfConnectionStrings();

                if (endpointName == "UsingMultipleNamespaces.EndpointInTargetNamespace")
                {
                    extensions.NamespaceRouting().AddNamespace("source", connectionString);
                    extensions.NamespacePartitioning().AddNamespace("target", targetConnectionString);
                }
                else
                {
                    extensions.NamespacePartitioning().AddNamespace("source", connectionString);
                    extensions.NamespaceRouting().AddNamespace("target", targetConnectionString);
                }
            };

            runSettings.Set("AzureServiceBus.AcceptanceTests.TransportConfigContext", ctx);

            var context = await Scenario.Define<Context>()
                .WithEndpoint<EndpointInSourceNamespace>(b =>
                {
                    b.When(async (bus, c) =>
                    {
                            var sendOptions = new SendOptions();
                            sendOptions.SetDestination("usingmultiplenamespaces.endpointintargetnamespace@target");
                            await bus.Send(new MyRequest(), sendOptions);
                    });
                })
                .WithEndpoint<EndpointInTargetNamespace>()
                .Done(c => c.ReplyReceived )
                .Run(runSettings);

            Assert.IsTrue(context.RequestReceived, "context.RequestReceived");
            Assert.IsTrue(context.ReplyReceived, "context.ReplyReceived");
        }


        [Test]
        public async Task Should_support_request_reply_across_namespaces_using_connection_strings()
        {
            var runSettings = new RunSettings();
            runSettings.TestExecutionTimeout = TimeSpan.FromMinutes(1);

            var connectionString = Environment.GetEnvironmentVariable("AzureServiceBusTransport.ConnectionString");
            var targetConnectionString = Environment.GetEnvironmentVariable("AzureServiceBus.ConnectionString.Fallback");
            
            var ctx = new AzureServiceBusTransportConfigContext();
            ctx.Callback = (endpointName, extensions) =>
            {
                   if (endpointName == "UsingMultipleNamespaces.EndpointInTargetNamespace")
                {
                    extensions.NamespaceRouting().AddNamespace("source", connectionString);
                    extensions.NamespacePartitioning().AddNamespace("target", targetConnectionString);
                }
                else
                {
                    extensions.NamespacePartitioning().AddNamespace("source", connectionString);
                    extensions.NamespaceRouting().AddNamespace("target", targetConnectionString);
                }
            };

            runSettings.Set("AzureServiceBus.AcceptanceTests.TransportConfigContext", ctx);

            var context = await Scenario.Define<Context>()
                .WithEndpoint<EndpointInSourceNamespace>(b =>
                {
                    b.When(async (bus, c) =>
                    {
                        var sendOptions = new SendOptions();
                        sendOptions.SetDestination("usingmultiplenamespaces.endpointintargetnamespace@" + targetConnectionString);
                        await bus.Send(new MyRequest(), sendOptions);
                    });
                })
                .WithEndpoint<EndpointInTargetNamespace>()
                .Done(c => c.ReplyReceived)
                .Run(runSettings);

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