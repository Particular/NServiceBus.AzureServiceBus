namespace NServiceBus.AzureServiceBus.AcceptanceTests.Infrastructure
{
    using System;
    using System.Threading.Tasks;
    using AcceptanceTesting;
    using Pipeline;
    using MessageMutator;

    class TestIndependenceMutator : IMutateOutgoingTransportMessages
    {
        string testRunId;

        public TestIndependenceMutator(ScenarioContext scenarioContext)
        {
            testRunId = scenarioContext.TestRunId.ToString();
        }

        public Task MutateOutgoing(MutateOutgoingTransportMessageContext context)
        {
            context.OutgoingHeaders["$AcceptanceTesting.TestRunId"] = testRunId;
            return Task.FromResult(0);
        }
    }

    class TestIndependenceSkipBehavior : Behavior<IIncomingPhysicalMessageContext>
    {
        string testRunId;

        public TestIndependenceSkipBehavior(ScenarioContext scenarioContext)
        {
            testRunId = scenarioContext.TestRunId.ToString();
        }

        public override Task Invoke(IIncomingPhysicalMessageContext context, Func<Task> next)
        {
            string runId;
            if (!context.MessageHeaders.TryGetValue("$AcceptanceTesting.TestRunId", out runId) || runId != testRunId)
            {
                Console.WriteLine($"Skipping message {context.MessageId} from previous test run");
                return Task.FromResult(0);
            }

            return next();
        }
    }
}