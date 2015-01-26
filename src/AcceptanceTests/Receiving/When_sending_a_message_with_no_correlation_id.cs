namespace NServiceBus.AcceptanceTests.WindowsAzureServiceBus
{
    using System;
    using EndpointTemplates;
    using AcceptanceTesting;
    using NServiceBus.Config;
    using NServiceBus.Config.ConfigurationSource;
    using NUnit.Framework;

    public class When_a_handler_fails_to_process_a_message : NServiceBusAcceptanceTest
    {  
        [Test]
        public void Should_abandon_the_message()
        {
            var context = new Context();

            Scenario.Define(context)
                    .WithEndpoint<MyEndpoint>(b => b.When(bus => bus.SendLocal(new MyRequest())))
                    .AllowExceptions()
                    .Done(c => c.ReceivedAgain)
                    .Run();
        }

        public class Context : ScenarioContext
        {
            public bool Thrown { get; set; }
            public bool ReceivedAgain { get; set; }
        }

        public class MyEndpoint : EndpointConfigurationBuilder
        {
            public MyEndpoint()
            {
                EndpointSetup<DefaultServer>();
            }

            public class SettingLockDurationLargerThanTestTimeout : IProvideConfiguration<AzureServiceBusQueueConfig>
            {
                public AzureServiceBusQueueConfig GetConfiguration()
                {
                    return new AzureServiceBusQueueConfig
                    {
                        LockDuration = 300000 // 5 mins, think test timeout is 2 minutes
                    };
                }
            }

            public class MyResponseHandler : IHandleMessages<MyRequest>
            {
                public Context Context { get; set; }

                public IBus Bus { get; set; }

                public void Handle(MyRequest response)
                {
                    if (!Context.Thrown)
                    {
                        Context.Thrown = true;
                        throw new Exception();
                    }

                    Context.ReceivedAgain = true;
                }
            }
        }


        [Serializable]
        public class MyRequest : IMessage
        {
        }
    }
}
