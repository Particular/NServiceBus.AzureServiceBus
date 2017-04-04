namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Routing
{
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;
    using Transport.AzureServiceBus;
    using X;
    using AcceptanceTesting.Customization;

    // When sending more than 100 messages in a batch and using AtomisSendsReceive mode
    public class When_sending_more_than_100_messages_in_a_transaction : NServiceBusAcceptanceTest
    {
        const int NumberOfMessagesToSendInAnAtomicBatch = 101;

        [Test]
        public Task Should_throw_a_custom_exception_and_move_the_incoming_message_to_the_error_queue()
        {
            return Scenario.Define<Context>()
                .WithEndpoint<Sender>(b => b.DoNotFailOnErrorMessages()
                    .When((session, ctx) => session.SendLocal(new KickOffCommand { Id = ctx.TestRunId })))
                .WithEndpoint<ErrorSpy>()
                .Done(c => c.MessageMovedToErrorQueue && c.ExceptionType == typeof(TransactionContainsTooManyMessages).FullName)
                .Run();
        }

        public class Context : ScenarioContext
        {
            public bool MessageMovedToErrorQueue { get; set; }
            public string ExceptionType { get; set; }
        }

        public class Sender : EndpointConfigurationBuilder
        {
            public Sender()
            {
                EndpointSetup<DefaultServer>(config =>
                {
                    var transport = config.UseTransport<AzureServiceBusTransport>();
                    transport.MessageSenders().MessageSizePaddingPercentage(0);
                    transport.UseNamespaceAliasesInsteadOfConnectionStrings();
                    config.SendFailedMessagesTo(Conventions.EndpointNamingConvention(typeof(ErrorSpy)));
                });
            }

            public class MyEventHandler : IHandleMessages<KickOffCommand>, IHandleMessages<Cmd>
            {
                public Context Context { get; set; }

                public async Task Handle(KickOffCommand messageThatIsEnlisted, IMessageHandlerContext context)
                {
                    for (var i = 0; i < NumberOfMessagesToSendInAnAtomicBatch; i++)
                    {
                        var sendOptions = new SendOptions();
                        // slim down messages as much as possible
                        sendOptions.SetMessageId("0");
                        sendOptions.SetHeader("NServiceBus.RelatedTo", string.Empty);
                        sendOptions.SetHeader("NServiceBus.ConversationId", string.Empty);
                        sendOptions.RouteToThisEndpoint();
                        await context.Send(new Cmd(), sendOptions).ConfigureAwait(false);
                    }
                }

                public Task Handle(Cmd message, IMessageHandlerContext context)
                {
                    return Task.FromResult(0);
                }
            }
        }

        public class ErrorSpy : EndpointConfigurationBuilder
        {
            public ErrorSpy()
            {
                EndpointSetup<DefaultServer>(config => config.LimitMessageProcessingConcurrencyTo(1));
            }

            class KickOffCommandMessageHandler : IHandleMessages<KickOffCommand>
            {
                public Context TestContext { get; set; }

                public Task Handle(KickOffCommand initiatingMessage, IMessageHandlerContext context)
                {
                    if (initiatingMessage.Id == TestContext.TestRunId)
                    {
                        TestContext.MessageMovedToErrorQueue = true;
                        TestContext.ExceptionType = context.MessageHeaders["NServiceBus.ExceptionInfo.ExceptionType"];
                    }

                    return Task.FromResult(0);
                }
            }
        }
    }
}

namespace X
{
    using System;
    using NServiceBus;

    public class KickOffCommand : ICommand
    {
        public Guid Id { get; set; }
    }
    public class Cmd : ICommand
    {
    }
}