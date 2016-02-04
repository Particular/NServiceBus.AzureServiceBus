namespace NServiceBus.AzureServiceBus.EnduranceTests.Commands
{
    using System;

    public class Heartbeat : ICommand
    {
        public Guid TestRunId { get; set; }
    }
}
