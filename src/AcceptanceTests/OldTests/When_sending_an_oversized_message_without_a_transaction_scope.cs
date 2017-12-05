namespace NServiceBus.AcceptanceTests.WindowsAzureServiceBus
{
    using System;
    using System.Threading.Tasks;
    using EndpointTemplates;
    using AcceptanceTesting;
    using Transport.AzureServiceBus;
    using NUnit.Framework;

    public class When_sending_an_oversized_message_without_a_transaction_scope : NServiceBusAcceptanceTest
    {  
        [Test]
        public async Task Should_throw_message_too_large_exception()
        {
            try
            {
                await Scenario.Define<Context>()
                   .WithEndpoint<MyEndpoint>(b => b.When(async bus => await bus.SendLocal(new OversizedRequest())))
                   .Run();
            }
            catch (MessageTooLargeException ex) when(ex is MessageTooLargeException)
            {
            }
        }

        class Context : ScenarioContext
        {
            public string MessageIdReceived { get; set; }
            public bool GotRequest { get; set; }
        }

        class MyEndpoint : EndpointConfigurationBuilder
        {
            public MyEndpoint()
            {
                EndpointSetup<DefaultServer>(c => c.Recoverability().Immediate(settings => settings.NumberOfRetries(0)));
            }
        }

        [Serializable]
        public class OversizedRequest : IMessage
        {
            public OversizedRequest()
            {
                OversizedProperty = new string('*', 265000);
            }

            public string OversizedProperty { get; set; }
        }
    }
}
