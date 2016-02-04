namespace NServiceBus.AzureServiceBus.EnduranceTests.Handlers
{
    using System.Linq;
    using System.Threading.Tasks;
    using NServiceBus.AzureServiceBus.EnduranceTests.Commands;
    using NServiceBus.Logging;

    public class HeartbeatHandler : IHandleMessages<Heartbeat>
    {
        static readonly ILog log = LogManager.GetLogger<HeartbeatHandler>();

        public async Task Handle(Heartbeat message, IMessageHandlerContext context)
        {
            log.InfoFormat("HeartbeatHandler Heartbeat with Wait of {0}", message.Wait);

            if (!TestSettings.TestRunIds.Contains(message.TestRunId))
            {
                return;
            }

            await Task.Delay(message.Wait);
            await context.Send<Heartbeat>(cmd =>
            {
                cmd.Wait = message.Wait;
                cmd.TestRunId = message.TestRunId;
            }, TestSettings.SendOptions);
        }
    }
}
