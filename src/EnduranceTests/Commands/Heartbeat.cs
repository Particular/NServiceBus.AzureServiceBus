namespace NServiceBus.AzureServiceBus.EnduranceTests.Commands
{
    using System;

    public class Heartbeat : ICommand
    {
        public int Wait { get; set; }
        public Guid TestRunId { get; set; }
    }
}
