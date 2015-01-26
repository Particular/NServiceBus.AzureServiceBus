namespace NServiceBus.AcceptanceTests.WindowsAzureServiceBus
{
    using System;
    using Azure.Transports.WindowsAzureServiceBus;
    using EndpointTemplates;
    using AcceptanceTesting;
    using NServiceBus.Config;
    using NServiceBus.Config.ConfigurationSource;
    using NUnit.Framework;

    public class When_sending_an_oversized_message_without_a_transaction_scope : NServiceBusAcceptanceTest
    {  
        [Test]
        public void Should_throw_message_too_large_exception()
        {
            var context = new Context();

            try
            {
                Scenario.Define(context)
                   .WithEndpoint<MyEndpoint>(b => b.When(bus => bus.SendLocal(new OversizedRequest())))
                   .Run();
            }
            catch (AggregateException ex)
            {
                var interesting = ex.InnerException.InnerException.InnerException;
                if (!(interesting is MessageTooLargeException))
                {
                    throw;
                }
            }
        }

        public class Context : ScenarioContext
        {
            public string MessageIdReceived { get; set; }
            public bool GotRequest { get; set; }
        }

        public class MyEndpoint : EndpointConfigurationBuilder
        {
            public MyEndpoint()
            {
                EndpointSetup<DefaultServer>();
            }

            public class ConfigMaxDeliveryCount : IProvideConfiguration<AzureServiceBusQueueConfig>
            {
                public AzureServiceBusQueueConfig GetConfiguration()
                {
                    return new AzureServiceBusQueueConfig
                    {
                        MaxDeliveryCount = 1
                    };
                }
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
