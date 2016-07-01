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

    public class When_using_single_namespace : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_append_namespace_name_to_reply_address_when_using_names()
        {
            var runSettings = new RunSettings();
            runSettings.TestExecutionTimeout = TimeSpan.FromMinutes(1);

            var ctx = new AzureServiceBusTransportConfigContext();
            ctx.Callback = (endpointName, extensions) =>
            {
                var connectionString = Environment.GetEnvironmentVariable("AzureServiceBusTransport.ConnectionString");
                extensions.ConnectionString(connectionString);
                extensions.UseNamespaceNamesInsteadOfConnectionStrings();
            };

            runSettings.Set("AzureServiceBus.AcceptanceTests.TransportConfigContext", ctx);

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
                .Run(runSettings);

            Assert.IsTrue(context.RequestReceived, "context.RequestReceived");
            Assert.IsTrue(context.ReplyReceived, "context.ReplyReceived");
            Assert.IsTrue(context.ReplyToContainsNamespace, "context.ReplyToContainsNamespace");
        }


        [Test]
        public async Task Should_not_append_namespace_name_to_reply_address_when_using_connection_strings()
        {
            var runSettings = new RunSettings();
            runSettings.TestExecutionTimeout = TimeSpan.FromMinutes(1);

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
                .Run(runSettings);

            Assert.IsTrue(context.RequestReceived, "context.RequestReceived");
            Assert.IsTrue(context.ReplyReceived, "context.ReplyReceived");
            Assert.IsFalse(context.ReplyToContainsNamespace, "context.ReplyToContainsNamespace");
        }

        public class SourceEndpoint : EndpointConfigurationBuilder
        {
            public SourceEndpoint()
            {
                EndpointSetup<DefaultServer>()
                    .AddMapping<MyRequest>(typeof(TargetEndpoint));
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
                EndpointSetup<DefaultServer>();
            }

            class MyRequestHandler : IHandleMessages<MyRequest>
            {
                public Context Context { get; set; }

                public async Task Handle(MyRequest message, IMessageHandlerContext context)
                {
                    Context.RequestReceived = true;
                    Context.ReplyToContainsNamespace = context.ReplyToAddress.Contains("@");
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
            public bool ReplyToContainsNamespace { get; set; }
        }
    }
}