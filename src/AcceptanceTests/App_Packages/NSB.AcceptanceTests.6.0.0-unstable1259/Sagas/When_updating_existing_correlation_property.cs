namespace NServiceBus.AcceptanceTests.Sagas
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.AcceptanceTesting.Support;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;

    public class When_updating_existing_correlation_property : NServiceBusAcceptanceTest
    {
        [Test]
        public void Should_blow_up()
        {
            // TODO: revert changes when idempotent-test-execution branch is merged

            var exception = Assert.Throws<AggregateException>(async () =>
                await Scenario.Define<Context>(context => context.Id = Guid.NewGuid())
                    .WithEndpoint<ChangePropertyEndpoint>(b => b.When((bus, ctx) => bus.SendLocal(new StartSagaMessage
                    {
                        SomeId = Guid.NewGuid(),
                        ContextId = ctx.Id
                    })))
                    .Done(c => c.Exceptions.Any())
                    .Run())
                .ExpectFailedMessages();

            Assert.AreEqual(1, exception.FailedMessages.Count);

            StringAssert.Contains(
                "Changing the value of correlated properties at runtime is currently not supported",
                exception.FailedMessages.Single().Exception.Message);
        }

        public class Context : ScenarioContext
        {
            public Guid Id { get; set; }
        }

        public class ChangePropertyEndpoint : EndpointConfigurationBuilder
        {
            public ChangePropertyEndpoint()
            {
                EndpointSetup<DefaultServer>();
            }

            public class ChangeCorrPropertySaga : Saga<ChangeCorrPropertySagaData>, IAmStartedByMessages<StartSagaMessage>
            {
                public Context TestContext { get; set; }

                public Task Handle(StartSagaMessage message, IMessageHandlerContext context)
                {
                    if (TestContext.Id != message.ContextId)
                    {
                        return Task.FromResult(0);
                    }

                    if (message.SecondMessage)
                    {
                        Data.SomeId = Guid.NewGuid(); //this is not allowed
                        return Task.FromResult(0);
                    }

                    return context.SendLocal(new StartSagaMessage
                    {
                        SecondMessage = true,
                        SomeId = Data.SomeId,
                        ContextId = TestContext.Id
                    });
                }

                protected override void ConfigureHowToFindSaga(SagaPropertyMapper<ChangeCorrPropertySagaData> mapper)
                {
                    mapper.ConfigureMapping<StartSagaMessage>(m => m.SomeId)
                        .ToSaga(s => s.SomeId);
                }
            }

            public class ChangeCorrPropertySagaData : IContainSagaData
            {
                public virtual Guid SomeId { get; set; }
                public virtual Guid Id { get; set; }
                public virtual string Originator { get; set; }
                public virtual string OriginalMessageId { get; set; }
            }
        }

        public class StartSagaMessage : ICommand
        {
            public Guid SomeId { get; set; }
            public bool SecondMessage { get; set; }
            public Guid ContextId { get; set; }
        }
    }
}