namespace NServiceBus.AzureServiceBus.EnduranceTests.Commands
{
    public class StartHeartbeat: ICommand
    {
        public int Wait { get; set; } = 4000;
    }
}