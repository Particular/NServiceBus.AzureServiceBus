namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.AcceptanceTests.Serialization
{
    using System.Threading.Tasks;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.AcceptanceTests;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NServiceBus.MessageMutator;
    using NUnit.Framework;

    public class When_using_default_serialization : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Serialization_should_set_to_JSON()
        {
            var context = await Scenario.Define<Context>()
                    .WithEndpoint<Sender>(b => b.When((session, ctx) => session.SendLocal(new MyMessage())))
                    .Done(ctx => ctx.SerializationDetected)
                    .Run();

            Assert.That(context.Serialization, Is.EqualTo("application/json"), $"Serialization should be `application/json`, but was `{context.Serialization}`.");
        }

        public class Context : ScenarioContext
        {
            public bool SerializationDetected { get; set; }
            public string Serialization { get; set; }
        }

        public class Sender : EndpointConfigurationBuilder
        {
            public Sender()
            {
                EndpointSetup<DefaultServer>(busConfiguration =>
                {
                    busConfiguration.RegisterComponents(components => components.ConfigureComponent<DetectDefaultSerializationMutator>(DependencyLifecycle.InstancePerCall));
                });
            }

            class DetectDefaultSerializationMutator : IMutateOutgoingTransportMessages
            {
                public Context Context { get; set; }

                public Task MutateOutgoing(MutateOutgoingTransportMessageContext context)
                {
                    Context.Serialization = context.OutgoingHeaders["NServiceBus.ContentType"];
                    Context.SerializationDetected = true;
                    return Task.FromResult(0);
                }
            }

            class NoopHandler : IHandleMessages<MyMessage>
            {
                public Task Handle(MyMessage message, IMessageHandlerContext context)
                {
                    return Task.FromResult(0); 
                }
            }
        }

        public class MyMessage : IMessage
        {
        }
    }
}
