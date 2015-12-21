namespace NServiceBus.AcceptanceTests.Config
{
    using System.Linq;
    using System.Threading.Tasks;
    using NServiceBus.AcceptanceTesting;
    using NServiceBus.AcceptanceTests.EndpointTemplates;
    using NServiceBus.AcceptanceTests.FakeTransport;
    using NServiceBus.Config;
    using NUnit.Framework;
    using NServiceBus.AcceptanceTests.ScenarioDescriptors;

    public class When_limiting_concurrency_via_both_api_and_config : NServiceBusAcceptanceTest
    {
        [Test]
        public async Task Should_throw()
        {
            // TODO: revert test change when resolve issue with the core not respecting transport override via EndpointSetup<DefaultServer>(c => c.UseTransport<FakeTransport>());

            await Scenario.Define<Context>()
                .WithEndpoint<ThrottledEndpoint>(b => b.CustomConfig(c => c.LimitMessageProcessingConcurrencyTo(10)))
                .Done(c => c.EndpointsStarted)
                .Repeat(r => r.For(Transports.AllAvailable.SingleOrDefault(t => t.Key == "FakeTransport")))
                .Should(context => context.Exceptions.First().Message.Contains("specified both via API and configuration"))
                .Run();
        }

        public class Context : ScenarioContext
        {
        }

        class ThrottledEndpoint : EndpointConfigurationBuilder
        {
            public ThrottledEndpoint()
            {
                EndpointSetup<DefaultServer>(c => c.UseTransport<FakeTransport>())
                    .WithConfig<TransportConfig>(c => c.MaximumConcurrencyLevel = 8);

            }
        }
    }
}