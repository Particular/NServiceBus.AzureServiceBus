namespace NServiceBus.AzureServiceBus.AcceptanceTests.Infrastructure
{
    using System;
    using System.Threading.Tasks;
    using NServiceBus.Pipeline;
    using NServiceBus.MessageMutator;

    public class TestIndependenceData
    {
        public string Header { get; } = "AcceptanceTesting.TestRunId";
        public string TestRunId { get; } = Guid.NewGuid().ToString();
    }

    class TestIndependenceMutator : IMutateOutgoingTransportMessages
    {
        TestIndependenceData data;

        public TestIndependenceMutator(TestIndependenceData data)
        {
            this.data = data;
        }

        public Task MutateOutgoing(MutateOutgoingTransportMessageContext context)
        {
            context.OutgoingHeaders[data.Header] = data.TestRunId;
            return Task.FromResult(0);
        }
    }

    class TestIndependenceSkipBehavior : Behavior<IncomingPhysicalMessageContext>
    {
        TestIndependenceData data;

        public TestIndependenceSkipBehavior(TestIndependenceData data)
        {
            this.data = data;
        }

        public override Task Invoke(IncomingPhysicalMessageContext context, Func<Task> next)
        {
            string runId;
            if (!context.MessageHeaders.TryGetValue(data.Header, out runId) || runId != data.TestRunId)
            {
                return Task.FromResult(0);
            }

            return next();
        }
    }


}