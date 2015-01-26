namespace NServiceBus.Features
{
    using Config;
    using Config.ConfigurationSource;
    using Settings;

    class OverrideMaxRetries : IProvideConfiguration<TransportConfig>
    {
        ReadOnlySettings settings;

        public OverrideMaxRetries(ReadOnlySettings settings)
        {
            this.settings = settings;
        }

        public TransportConfig GetConfiguration()
        {
            var source = settings.Get<IConfigurationSource>();
            
            var c = source.GetConfiguration<AzureServiceBusQueueConfig>();
            var t = source.GetConfiguration<TransportConfig>();

            if (c == null)
            {
                c = new AzureServiceBusQueueConfig();
            }
            if (t == null)
            {
                t = new TransportConfig();
            }

            return new TransportConfig
                        {
                            MaximumConcurrencyLevel = t.MaximumConcurrencyLevel,
                            MaxRetries = t.MaxRetries >= c.MaxDeliveryCount - 1 ? c.MaxDeliveryCount - 2 : t.MaxRetries,
                            MaximumMessageThroughputPerSecond = t.MaximumMessageThroughputPerSecond
                        };
        }
    }
}