namespace NServiceBus.AzureServiceBus.EnduranceTests.Commands
{
    public class Heartbeat : ICommand
    {
        public int Wait { get; set; }
    }
}
