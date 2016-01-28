namespace NServiceBus.AzureServiceBus.EnduranceTests.Commands
{
    using System;

    public class StartHeartbeat: ICommand
    {
        public int Wait { get; set; } = 4000;
        public Guid TestRunId { get; set; }
    }
}