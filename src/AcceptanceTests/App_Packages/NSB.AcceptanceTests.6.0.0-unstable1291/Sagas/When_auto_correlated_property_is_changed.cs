namespace NServiceBus.AcceptanceTests.Sagas
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.AcceptanceTesting.Support;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NUnit.Framework;

    [TestFixture]
    public class When_auto_correlated_property_is_changed : NServiceBusAcceptanceTest
    {
        [Test]
        public void Should_throw()
        {
            // TODO: revert changes when idempotent-test-execution branch is merged

            var exception = Assert.Throws<AggregateException>(async () =>
                await Scenario.Define<Context>()
                    .WithEndpoint<Endpoint>(
                        b => b.When((bus, ctx) =>
                        {
                            ctx.Id = Guid.NewGuid();
                            return bus.SendLocal(new StartSaga { DataId = ctx.Id });
                        }))
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

        public class Endpoint : EndpointConfigurationBuilder
        {
            public Endpoint()
            {
                EndpointSetup<DefaultServer>();
            }

            public class CorrIdChangedSaga : Saga<CorrIdChangedSaga.CorrIdChangedSagaData>,
                IAmStartedByMessages<StartSaga>
            {
                public Context TestContext { get; set; }

                public Task Handle(StartSaga message, IMessageHandlerContext context)
                {
                    if (TestContext.Id != message.DataId)
                    {
                        return Task.FromResult(0);
                    }

                    Data.DataId = Guid.NewGuid();
                    return Task.FromResult(0);
                }

                protected override void ConfigureHowToFindSaga(SagaPropertyMapper<CorrIdChangedSagaData> mapper)
                {
                    mapper.ConfigureMapping<StartSaga>(m => m.DataId).ToSaga(s => s.DataId);
                }

                public class CorrIdChangedSagaData : ContainSagaData
                {
                    public virtual Guid DataId { get; set; }
                }
            }
        }

        public class StartSaga : ICommand
        {
            public Guid DataId { get; set; }
        }
    }
}