namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Addressing
{
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using AcceptanceTesting.Customization;
    using AzureServiceBus;
    using AzureServiceBus.AcceptanceTests.Infrastructure;
    using Microsoft.ServiceBus;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;

    public class When_sending_message_with_hierarchy_composition_strategy : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_send_to_one_namespace_only()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<SourceEndpoint>(b =>
                {
                    b.When(async (bus, c) =>
                    {
                        await bus.Send(new MyRequest());
                    });
                })
                .WithEndpoint<TargetEndpoint>()
                .Done(c => c.RequestsReceived == 1 && c.ResponsesReceived == 1)
                .Run();

            var namespaceManager = NamespaceManager.CreateFromConnectionString(TestUtility.DefaultConnectionString);
            var queueNames = context.ReplyToAddresses.Select(x => x.Replace("@default", "")).ToArray();

            var results = await Task.WhenAll(namespaceManager.QueueExistsAsync(queueNames[0]), namespaceManager.QueueExistsAsync(queueNames[1]));

            CollectionAssert.AreEquivalent(new[] { true, true }, results, "'{0}' and '{1}' queues were expected to exist, but were not found.", queueNames);
        }


        public class SourceEndpoint : EndpointConfigurationBuilder
        {
            public SourceEndpoint()
            {
                EndpointSetup<DefaultServer>(c =>
                {
                    var transport = c.ConfigureAzureServiceBus();
                    transport.Composition().UseStrategy<HierarchyComposition>().PathGenerator(path => "scadapter/");

                    c.ConfigureTransport().Routing().RouteToEndpoint(typeof(MyRequest), typeof(TargetEndpoint));
                });
            }

            class MyResponseHandler : IHandleMessages<MyResponse>
            {
                public Context Context { get; set; }

                public Task Handle(MyResponse message, IMessageHandlerContext context)
                {
                    Context.ReplyToAddresses.Push(context.ReplyToAddress);
                    Context.ReceivedResponse();
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
                    transport.Composition().UseStrategy<HierarchyComposition>().PathGenerator(path => "scadapter/");
                });
            }

            class MyRequestHandler : IHandleMessages<MyRequest>
            {
                public Context Context { get; set; }

                public Task Handle(MyRequest message, IMessageHandlerContext context)
                {
                    Context.ReplyToAddresses.Push(context.ReplyToAddress);
                    Context.ReceivedRequest();
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
            long receivedRequest;
            long receivedResponse;

            public Context()
            {
                ReplyToAddresses = new ConcurrentStack<string>();
            }

            public long RequestsReceived => Interlocked.Read(ref receivedRequest);
            public long ResponsesReceived => Interlocked.Read(ref receivedResponse);

            public ConcurrentStack<string> ReplyToAddresses { get; }

            public void ReceivedRequest() => Interlocked.Increment(ref receivedRequest);
            public void ReceivedResponse() => Interlocked.Increment(ref receivedResponse);
        }
    }
}