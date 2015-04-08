namespace NServiceBus.AcceptanceTests.WindowsAzureServiceBus
{
    using System;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NServiceBus.Config;
    using NServiceBus.Config.ConfigurationSource;
    using NUnit.Framework;

    public class When_overriding_endpoint_name : NServiceBusAcceptanceTest
    {
        [Test]
        public void Should_deliver_messages_to_destination()
        {
            var context = new Context
            {
                Id = Guid.NewGuid()
            };

            Scenario.Define(context)
                .WithEndpoint<SendingEndpoint>(builder => builder.When(bus => bus.Send(new SomeCommand
                {
                    ContextId = context.Id
                })))
                .WithEndpoint<ReceivingEndpoint>()
                .Run();

            Assert.True(context.MessageReceived);
        }


        [Serializable]
        public class SomeCommand : ICommand
        {
            public Guid ContextId { get; set; }
        }

        public class SendingEndpoint : EndpointConfigurationBuilder
        {
            public SendingEndpoint()
            {
                EndpointSetup<DefaultServer>()
                    .AddMapping<SomeCommand>(typeof(ReceivingEndpoint));
            }

            class OverrideAzureServiceBusQueueName : IProvideConfiguration<AzureServiceBusQueueConfig>
            {
                public AzureServiceBusQueueConfig GetConfiguration()
                {
                    return new AzureServiceBusQueueConfig
                    {
                        QueueName = "kaboom"
                    };
                }
            }
        }

        public class ReceivingEndpoint : EndpointConfigurationBuilder, IHandleMessages<SomeCommand>
        {
            public Context Context { get; set; }

            public ReceivingEndpoint()
            {
                EndpointSetup<DefaultServer>();
            }

            public void Handle(SomeCommand message)
            {
                if (message.ContextId == Context.Id)
                {
                    Context.MessageReceived = true;
                }
            }
        }

        public class Context : ScenarioContext
        {
            public Guid Id { get; set; }
            public bool MessageReceived { get; set; }
        }
    }
}