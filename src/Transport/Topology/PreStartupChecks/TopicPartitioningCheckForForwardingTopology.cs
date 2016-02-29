namespace NServiceBus.AzureServiceBus
{
    using System.Threading.Tasks;
    using NServiceBus.Settings;
    using NServiceBus.Transports;

    class TopicPartitioningCheckForForwardingTopology
    {
        private readonly ReadOnlySettings setting;

        public TopicPartitioningCheckForForwardingTopology(ReadOnlySettings setting)
        {
            this.setting = setting;
        }

        public Task<StartupCheckResult> Run()
        {
            if (setting.Get<bool>(WellKnownConfigurationKeys.Topology.Resources.Topics.EnablePartitioning))
            {
                return Task.FromResult(StartupCheckResult.Failed($"When using `{typeof(ForwardingTopology).Name}`, topic partitioning should not be enabled. Disable topic partitioning by removing `.EnablePartitioning(true);` or calling `.EnablePartitioning(false);` in transport configuration."));
            }
            
            return Task.FromResult(StartupCheckResult.Success);
        }
    }
}