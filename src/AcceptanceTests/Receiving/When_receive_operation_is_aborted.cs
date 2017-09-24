namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Routing
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;
    using AcceptanceTesting.Customization;
    using Pipeline;

    public class When_receive_operation_is_aborted : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_not_dispatch_outgoing_message()
        {
            var delay = Task.Delay(TimeSpan.FromSeconds(30));

            var context = await Scenario.Define<Context>()
                .WithEndpoint<AbortReceivingEndpoint>(builder => builder.When((session, ctx) => session.SendLocal(new InitialMessage())))
                .WithEndpoint<EndpointThatShouldNotReceive>()
                .Done(ctx => delay.IsCompleted || ctx.TimesDispatchedMessageReceived > 0)
                .Run();
            
            Assert.That(context.TimesDispatchedMessageReceived, Is.Zero);
        }

        public class Context : ScenarioContext
        {
            public int TimesDispatchedMessageReceived { get; set; }
            public int TimesSenderHandlerInvoked { get; set; }
        }

        public class AbortReceivingEndpoint : EndpointConfigurationBuilder
        {
            public AbortReceivingEndpoint()
            {
                EndpointSetup<DefaultServer>(config =>
                {
                    var transport = config.UseTransport<AzureServiceBusTransport>();
                    transport.Routing().RouteToEndpoint(typeof(DispatchedMessage), typeof(EndpointThatShouldNotReceive));
                    config.LimitMessageProcessingConcurrencyTo(1);

                    config.Pipeline.Register("AbortReceiveOperation", typeof(AbortReceiveOperationBehavior), "Abort receive operation");
                });
            }

            public class InitialMessageHandler : IHandleMessages<InitialMessage>
            {
                public Context Context { get; set; }

                public async Task Handle(InitialMessage initialMessage, IMessageHandlerContext context)
                {
                    if (Context.TimesSenderHandlerInvoked == 0)
                    {
                        await context.Send(new DispatchedMessage { Id = Context.TestRunId });
                    }
                    Context.TimesSenderHandlerInvoked++;
                }
            }

            class AbortReceiveOperationBehavior : IBehavior<ITransportReceiveContext, ITransportReceiveContext>
            {
                string testRunId;

                public AbortReceiveOperationBehavior(ScenarioContext scenarioContext)
                {
                    testRunId = scenarioContext.TestRunId.ToString();
                }
                public async Task Invoke(ITransportReceiveContext context, Func<ITransportReceiveContext, Task> next)
                {
                    await next(context);

                    if (context.Message.Headers.TryGetValue("$AcceptanceTesting.TestRunId", out var runId) && runId == testRunId)
                    {
                        context.AbortReceiveOperation();
                    }
                }
            }
        }

        public class EndpointThatShouldNotReceive : EndpointConfigurationBuilder
        {
            public EndpointThatShouldNotReceive()
            {
                EndpointSetup<DefaultServer>(config =>
                {

                });
            }

            public class DispatchedMessageHandler : IHandleMessages<DispatchedMessage>
            {
                public Context Context { get; set; }

                public Task Handle(DispatchedMessage message, IMessageHandlerContext context)
                {
                    Context.TimesDispatchedMessageReceived++;
                    return Task.FromResult(0);
                }
            }
        }

        public class InitialMessage : IMessage
        {
        }

        public class DispatchedMessage : IMessage
        {
            public Guid Id { get; set; }
        }
    }
}