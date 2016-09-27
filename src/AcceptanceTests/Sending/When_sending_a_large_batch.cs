namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Routing
{
    using System.Threading;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;
    using X;

    // When sending more than 100 messages in a batch and using AtomisSendsReceive mode
    public class When_sending_a_large_batch : NServiceBusAcceptanceTest
    {
        const int NumberOfMessagesToSendInAnAtomicBatch = 101;

        [Test]
        public async Task Should_not_fail_for_a_batch_with_more_than_100_messages()
        {
            await Scenario.Define<Context>()
                .WithEndpoint<Sender>(b => b.When(c => c.EndpointsStarted, async session =>
                {
                    await session.SendLocal(new KickOffCommand()).ConfigureAwait(false);
                }))
                .Done(c => c.NumberOfReceivedMessages == NumberOfMessagesToSendInAnAtomicBatch)
                .Run();
        }

        public class Context : ScenarioContext
        {
            int numberOfReceivedMessages;

            public int NumberOfReceivedMessages => numberOfReceivedMessages;

            public void IncrementNumberOfReceivedMessages()
            {
                Interlocked.Increment(ref numberOfReceivedMessages);
            }
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
                        sendOptions.SetCorrelationId("0");
                        sendOptions.SetHeader("NServiceBus.RelatedTo", string.Empty);
                        sendOptions.SetHeader("NServiceBus.ConversationId", string.Empty);
                        sendOptions.RouteToThisEndpoint();
                        await context.Send(new Cmd(), sendOptions).ConfigureAwait(false);
                    }
                }

                public Task Handle(Cmd message, IMessageHandlerContext context)
                {
                    Context.IncrementNumberOfReceivedMessages();
                    return Task.FromResult(0);
                }
            }
        }

        
    }
}

namespace X
{
    using NServiceBus;

    public class KickOffCommand : ICommand
    {
    }
    public class Cmd : ICommand
    {
    }
}