namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Routing
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using AcceptanceTesting.Support;
    using AzureServiceBus;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;

    public class When_using_multiple_namespaces : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_support_request_reply_across_namespaces()
        {
            var runSettings = new RunSettings();
            runSettings.TestExecutionTimeout = TimeSpan.FromMinutes(1);

            var ctx = new AzureServiceBusTransportConfigContext();
            ctx.Callback = (endpointName, extensions) =>
            {
                var connectionString = Environment.GetEnvironmentVariable("AzureServiceBusTransport.ConnectionString");
                var targetConnectionString = Environment.GetEnvironmentVariable("AzureServiceBus.ConnectionString.Fallback");
                extensions.UseNamespaceNamesInsteadOfConnectionStrings();

                if (endpointName == "UsingMultipleNamespaces.EndpointInTargetNamespace")
                {
                    extensions.AddDestinationNamespace("source", connectionString);
                    extensions.AddDestinationNamespace("target", targetConnectionString);
                }
                else
                {
                    extensions.AddPartitioningNamespace("source", connectionString);
                    extensions.AddPartitioningNamespace("target", targetConnectionString);
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

        public class EndpointInSourceNamespace : EndpointConfigurationBuilder
        {
            public EndpointInSourceNamespace()
            {
                EndpointSetup<DefaultServer>()
                    .AddMapping<MyRequest>(typeof(EndpointInTargetNamespace));
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

                public async Task Handle(MyRequest message, IMessageHandlerContext context)
                {
                    Context.RequestReceived = true;
                    await context.Reply(new MyResponse());
                }
            }
        }

        class MyRequest : IMessage
        {
        }

        class MyResponse : IMessage
        {
        }

        class Context : ScenarioContext
        {
            public bool RequestReceived { get; set; }
            public bool ReplyReceived { get; set; }
        }
    }

    public class AzureServiceBusTransportConfigContext
    {
       public Action<string, TransportExtensions< AzureServiceBusTransport>> Callback
       {
           get;
           set;
       }
    }
}