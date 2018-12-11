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
            var id = (scenarioContext as IProvideTestRunId)?.OverrideTestRunId ?? scenarioContext.TestRunId;
            testRunId = id.ToString();
        }

        public Task MutateOutgoing(MutateOutgoingTransportMessageContext context)
        {
            context.OutgoingHeaders["$AcceptanceTesting.TestRunId"] = testRunId;
            return Task.FromResult(0);
        }
    }

    class TestIndependenceSkipBehavior : IBehavior<ITransportReceiveContext, ITransportReceiveContext>
    {
        string testRunId;

        public TestIndependenceSkipBehavior(ScenarioContext scenarioContext)
        {
            var id = (scenarioContext as IProvideTestRunId)?.OverrideTestRunId ?? scenarioContext.TestRunId;
            testRunId = id.ToString();
        }

        public Task Invoke(ITransportReceiveContext context, Func<ITransportReceiveContext, Task> next)
        {
            if (!context.Message.Headers.TryGetValue("$AcceptanceTesting.TestRunId", out var runId) || runId != testRunId)
            {
                Console.WriteLine($"Skipping message {context.Message.MessageId} from previous test run");
                return Task.FromResult(0);
            }

            return next(context);
        }
    }

    interface IProvideTestRunId
    {
        Guid? OverrideTestRunId { get; }
    }
}