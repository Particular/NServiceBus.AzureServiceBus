namespace NServiceBus.AzureServiceBus.EnduranceTests.Handlers
{
    using System.Threading.Tasks;
    using Commands;
    using Logging;

    public class HeartbeatHandler : IHandleMessages<Heartbeat>
    {
        static readonly ILog log = LogManager.GetLogger<HeartbeatHandler>();

        public async Task Handle(Heartbeat message, IMessageHandlerContext context)
        {
            log.InfoFormat("HeartbeatHandler Heartbeat with Wait of {0}", TestSettings.Rate);

            if (TestSettings.TestRunId != message.TestRunId)
            {
                return;
            }

            await Task.Delay(TestSettings.Rate);
            await context.Send<Heartbeat>(cmd =>
            {
                cmd.TestRunId = TestSettings.TestRunId;
            }, TestSettings.SendOptions);
        }
    }
}
