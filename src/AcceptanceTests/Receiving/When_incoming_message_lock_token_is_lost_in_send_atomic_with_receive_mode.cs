namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Routing
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;

    public class When_incoming_message_lock_token_is_lost_in_send_atomic_with_receive_mode : NServiceBusAcceptanceTest
    {
        const int LockDurationOnIncomingMessageInSeconds = 5;

        [Test]
        public async Task Should_not_dispatch_outgoing_message()
        {
            var context = await Scenario.Define<Context>()
                .WithEndpoint<Sender>(builder => builder.When((session, ctx) => session.SendLocal(new InitialMessage())))
                .WithEndpoint<Receiver>()
                .Done(ctx => ctx.TimesSenderHandlerInvoked > 0)
                .Run();

            Assert.That(context.TimesDispatchedMessageReceived, Is.Zero);
        }

        public class Context : ScenarioContext
        {
            public int TimesDispatchedMessageReceived { get; set; }
            public int TimesSenderHandlerInvoked { get; set; }
        }

        public class Sender : EndpointConfigurationBuilder
        {
            public Sender()
            {
                EndpointSetup<DefaultServer>(config =>
                {
                    var transport = config.UseTransport<AzureServiceBusTransport>();
                    transport.Queues().LockDuration(TimeSpan.FromSeconds(LockDurationOnIncomingMessageInSeconds));
                    transport.Queues().MaxDeliveryCount(100);
                    transport.MessageReceivers().AutoRenewTimeout(TimeSpan.Zero);
                    transport.Routing().RouteToEndpoint(typeof(DispatchedMessage), ConfigureEndpointAzureServiceBusTransport.NameForEndpoint<Receiver>());
                    config.LimitMessageProcessingConcurrencyTo(1);
#pragma warning disable 618
                    config.Recoverability().DisableLegacyRetriesSatellite();
#pragma warning restore 618
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
                        await Task.Delay(TimeSpan.FromSeconds(LockDurationOnIncomingMessageInSeconds * 2));
                    }
                    Context.TimesSenderHandlerInvoked++;
                }
            }
        }
        public class Receiver : EndpointConfigurationBuilder
        {
            public Receiver()
            {
                EndpointSetup<DefaultServer>(config =>
                {
#pragma warning disable 618
                    config.Recoverability().DisableLegacyRetriesSatellite();
#pragma warning restore 618
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