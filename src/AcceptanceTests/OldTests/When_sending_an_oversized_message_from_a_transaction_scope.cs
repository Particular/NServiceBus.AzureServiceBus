namespace NServiceBus.AcceptanceTests.WindowsAzureServiceBus
{
    using System;
    using System.Threading.Tasks;
    using System.Transactions;
    using EndpointTemplates;
    using AcceptanceTesting;
    using NUnit.Framework;
    using Transport.AzureServiceBus;

    public class When_sending_an_oversized_message_from_a_transaction_scope : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_throw_message_too_large_exception()
        {
            try
            {
                await Scenario.Define<Context>()
                   .WithEndpoint<MyEndpoint>(b => b.When(async bus =>
                   {
                       using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew))
                       {
                           await bus.SendLocal(new OversizedRequest());
                           scope.Complete();
                       }
                   }))
                   .Run();
            }
            // ReSharper disable once UnusedVariable
            catch (Exception ex) when (ex is MessageTooLargeException)
            {
            }
        }

        private class Context : ScenarioContext
        {
        }

        private class MyEndpoint : EndpointConfigurationBuilder
        {
            public MyEndpoint()
            {
                EndpointSetup<DefaultServer>(config => config.UseTransport<AzureServiceBusTransport>().Queues().MaxDeliveryCount(1));
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
