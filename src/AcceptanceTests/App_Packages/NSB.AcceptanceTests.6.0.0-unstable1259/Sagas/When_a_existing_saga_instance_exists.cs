namespace NServiceBus.AcceptanceTests.Sagas
{
    using System;
    using System.Threading.Tasks;
    using EndpointTemplates;
    using AcceptanceTesting;
    using NUnit.Framework;

    public class When_a_existing_saga_instance_exists : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_hydrate_and_invoke_the_existing_instance()
        {
            // TODO: revert changes when idempotent-test-execution branch is merged

            var context = await Scenario.Define<Context>()
                .WithEndpoint<ExistingSagaInstanceEndpt>(b => b.When((bus, ctx) =>
                {
                    ctx.Id = Guid.NewGuid();
                    return bus.SendLocal(new StartSagaMessage
                    {
                        SomeId = Guid.NewGuid(),
                        TestRunId = ctx.Id
                    });
                }))
                .Done(c => c.SecondMessageReceived)
                .Run();

            Assert.AreEqual(context.FirstSagaId, context.SecondSagaId, "The same saga instance should be invoked invoked for both messages");
        }

        public class Context : ScenarioContext
        {
            public bool SecondMessageReceived { get; set; }
            public Guid FirstSagaId { get; set; }
            public Guid SecondSagaId { get; set; }
            public Guid Id { get; set; }
        }

        public class ExistingSagaInstanceEndpt : EndpointConfigurationBuilder
        {
            public ExistingSagaInstanceEndpt()
            {
                EndpointSetup<DefaultServer>();
            }

            public class TestSaga05 : Saga<TestSagaData05>, IAmStartedByMessages<StartSagaMessage>
            {
                public Context TestContext { get; set; }

                public Task Handle(StartSagaMessage message, IMessageHandlerContext context)
                {
                    if (TestContext.Id != message.TestRunId)
                    {
                        return Task.FromResult(0);
                    }

                    Data.SomeId = message.SomeId;

                    if (message.SecondMessage)
                    {
                        TestContext.SecondSagaId = Data.Id;
                        TestContext.SecondMessageReceived = true;
                    }
                    else
                    {
                        TestContext.FirstSagaId = Data.Id;
                        return context.SendLocal(new StartSagaMessage
                        {
                            SomeId = message.SomeId,
                            SecondMessage = true,
                            TestRunId = TestContext.Id
                        });
                    }

                    return Task.FromResult(0);
                }

                protected override void ConfigureHowToFindSaga(SagaPropertyMapper<TestSagaData05> mapper)
                {
                    mapper.ConfigureMapping<StartSagaMessage>(m => m.SomeId)
                        .ToSaga(s => s.SomeId);
                }
            }

            public class TestSagaData05 : IContainSagaData
            {
                public virtual Guid Id { get; set; }
                public virtual string Originator { get; set; }
                public virtual string OriginalMessageId { get; set; }
                public virtual Guid SomeId { get; set; }
            }
        }

        [Serializable]
        public class StartSagaMessage : ICommand
        {
            public Guid SomeId { get; set; }

            public bool SecondMessage { get; set; }

            public Guid TestRunId { get; set; }
        }
    }
}