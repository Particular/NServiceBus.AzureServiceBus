namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Addressing
{
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using AcceptanceTesting.Customization;
    using AzureServiceBus;
    using AzureServiceBus.AcceptanceTests.Infrastructure;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;

    public class When_sending_message_with_default_partitioning_strategy : NServiceBusAcceptanceTest
    {
        static string connectionString = connectionString = TestUtility.DefaultConnectionString;
        static string targetConnectionString = TestUtility.FallbackConnectionString;

        [Test]
        public async Task Should_send_to_one_namespace_only()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<SourceEndpoint>(b =>
                {
                    b.When(async (bus, c) =>
                    {
                        await bus.Send(new MyRequest());
                        await bus.Send(new MyRequest());
                    });
                })
                .WithEndpoint<TargetEndpoint>(b => b.CustomConfig(c => c.ConfigureAzureServiceBus().ConnectionString(connectionString)))
                .WithEndpoint<TargetEndpoint>(b => b.CustomConfig(c => c.ConfigureAzureServiceBus().ConnectionString(targetConnectionString)))
                .Done(c => c.RequestsReceived == 2 && c.NamespaceNames.Count == 2)
                .Run();

            CollectionAssert.AreEquivalent(new[]
            {
                $"{Conventions.EndpointNamingConvention(typeof(TargetEndpoint))}@namespace1",
                $"{Conventions.EndpointNamingConvention(typeof(TargetEndpoint))}@namespace1",
            }, context.NamespaceNames);
        }


        public class SourceEndpoint : EndpointConfigurationBuilder
        {
            public SourceEndpoint()
            {
                EndpointSetup<DefaultServer>(c =>
                {
                    var transport = c.ConfigureAzureServiceBus();
                    var partitioning = transport.NamespacePartitioning();
                    partitioning.UseStrategy<SingleNamespacePartitioning>();
                    partitioning.AddNamespace("namespace1", connectionString);

                    c.ConfigureTransport().Routing().RouteToEndpoint(typeof(MyRequest), typeof(TargetEndpoint));
                });
            }

            class MyResponseHandler : IHandleMessages<MyResponse>
            {
                public Context Context { get; set; }

                public Task Handle(MyResponse message, IMessageHandlerContext context)
                {
                    Context.NamespaceNames.Add(context.ReplyToAddress);
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

                public Task Handle(MyRequest message, IMessageHandlerContext context)
                {
                    Context.Received();
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
            long received;
            public Context()
            {
                NamespaceNames = new ConcurrentBag<string>();
            }

            public long RequestsReceived => Interlocked.Read(ref received);

            public ConcurrentBag<string> NamespaceNames { get; }

            public void Received() => Interlocked.Increment(ref received);
        }
    }
}