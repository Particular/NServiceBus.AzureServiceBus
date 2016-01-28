namespace NServiceBus.AzureServiceBus.EnduranceTests.Handlers
{
    using System.Threading.Tasks;
    using NServiceBus.AzureServiceBus.EnduranceTests.Commands;
    using NServiceBus.Logging;

    public class StartHeartbeatHandler : IHandleMessages<StartHeartbeat>
    {
        private static readonly ILog log = LogManager.GetLogger<StartHeartbeatHandler>();
        public Task Handle(StartHeartbeat message, IMessageHandlerContext context)
        {
            log.InfoFormat("StartHeartbeatHandler starting Heartbeat with interval of {0}", message.Wait);
            TestSettings.TestEnabled = true;
            return context.Send<Heartbeat>(cmd => { cmd.Wait = message.Wait; }, TestSettings.SendOptions);
        }
    }
}