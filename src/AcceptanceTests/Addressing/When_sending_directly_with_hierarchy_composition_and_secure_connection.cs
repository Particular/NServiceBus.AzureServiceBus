namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Addressing
{
    using System.Threading;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using AcceptanceTesting.Customization;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;

    public class When_sending_directly_with_hierarchy_composition_and_secure_connection : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_send_and_receive_message()
        {
            await Scenario.Define<Context>()
                .WithEndpoint<SourceEndpoint>(b =>
                {
                    b.When(async (bus, c) =>
                    {
                        var sendOptions = new SendOptions();
                        sendOptions.SetDestination($"{Conventions.EndpointNamingConvention(typeof(TargetEndpoint))}@default");
                        await bus.Send(new MyRequest(), sendOptions);
                    });
                })
                .WithEndpoint<TargetEndpoint>()
                .Done(c => c.RequestsReceived == 1)
                .Run();
        }


        public class SourceEndpoint : EndpointConfigurationBuilder
        {
            public SourceEndpoint()
            {
                EndpointSetup<DefaultServer>(c =>
                {
                    var transport = c.ConfigureAzureServiceBus();
                    transport.Composition().UseStrategy<HierarchyComposition>().PathGenerator(path => "scadapter/");
                    transport.UseNamespaceAliasesInsteadOfConnectionStrings();
                });
            }
        }

        public class TargetEndpoint : EndpointConfigurationBuilder
        {
            public TargetEndpoint()
            {
                EndpointSetup<DefaultServer>(c =>
                {
                    var transport = c.ConfigureAzureServiceBus();
                    transport.Composition().UseStrategy<HierarchyComposition>().PathGenerator(path => "scadapter/");
                    transport.UseNamespaceAliasesInsteadOfConnectionStrings();
                });
            }

            class MyRequestHandler : IHandleMessages<MyRequest>
            {
                public Context Context { get; set; }

                public Task Handle(MyRequest message, IMessageHandlerContext context)
                {
                    Context.ReceivedRequest();
                    return Task.FromResult(0);
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
            long receivedRequest;

            public long RequestsReceived => Interlocked.Read(ref receivedRequest);

            public void ReceivedRequest()
            {
                Interlocked.Increment(ref receivedRequest);
            }
        }
    }
}