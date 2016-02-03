namespace NServiceBus.AzureServiceBus
{
    using System;
    using NServiceBus.Settings;
    using NServiceBus.Transports;
    using System.Collections.Generic;
    using System.Linq;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.Routing;

    public interface ITopology {

        void Initialize(SettingsHolder settings);

        Func<ICreateQueues> GetQueueCreatorFactory();
        // TODO: CriticalError no longer passed in to MessagePumpFactory. Ensure that Core is doing pushMessages.OnCriticalError(error);
        Func<IPushMessages> GetMessagePumpFactory();
        Func<IDispatchMessages> GetDispatcherFactory();
        StartupCheckResult ApplyPreStartupChecks();
        IManageSubscriptions GetSubscriptionManager();
        OutboundRoutingPolicy GetOutboundRoutingPolicy();
        bool HasNativePubSubSupport { get; }
        bool HasSupportForCentralizedPubSub { get;}
    }

    // NOTE: where do we move this components?
    public interface IStartupCheck
    {
        // NOTE: to make awaitable?
        StartupCheckResult Apply();
    }

    class CompositeStartupCheck : IStartupCheck
    {
        private readonly IEnumerable<IStartupCheck> _checks;

        public CompositeStartupCheck(IEnumerable<IStartupCheck> checks)
        {
            _checks = checks;
        }

        public StartupCheckResult Apply()
        {
            foreach (var check in _checks)
            {
                var result = check.Apply();
                if (!result.Succeeded)
                    return result;
            }

            return StartupCheckResult.Success;
        }
    }

    class NamespacesConfigurationCheck : IStartupCheck
    {
        private readonly INamespacePartitioningStrategy namespacePartitioningStrategy;
        private readonly ReadOnlySettings settings;

        public NamespacesConfigurationCheck(INamespacePartitioningStrategy namespacePartitioningStrategy, ReadOnlySettings settings)
        {
            this.namespacePartitioningStrategy = namespacePartitioningStrategy;
            this.settings = settings;
        }

        public StartupCheckResult Apply()
        {
            var namespaces = settings.Get<List<string>>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);
            var endpointName = settings.Get<EndpointName>();
            
            var configuredNamespaces = namespacePartitioningStrategy.GetNamespaces(endpointName.ToString(), PartitioningIntent.Creating);

            return namespaces.Count != configuredNamespaces.Count() ?
                StartupCheckResult.Failed("...") : StartupCheckResult.Success;
        }
    }

    class ManageRightsCheck : IStartupCheck
    {
        private readonly IManageNamespaceManagerLifeCycle manageNamespaceManagerLifeCycle;
        private readonly ReadOnlySettings settings;

        public ManageRightsCheck(IManageNamespaceManagerLifeCycle manageNamespaceManagerLifeCycle, ReadOnlySettings settings)
        {
            this.manageNamespaceManagerLifeCycle = manageNamespaceManagerLifeCycle;
            this.settings = settings;
        }

        public StartupCheckResult Apply()
        {
            if (!settings.Get<bool>(WellKnownConfigurationKeys.Core.CreateTopology))
                return StartupCheckResult.Success;

            var hasNotRights = settings.Get<List<string>>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces)
                .Select(x => manageNamespaceManagerLifeCycle.Get(x))
                .Any(x => !x.HasManageRights);

            return hasNotRights ?
                StartupCheckResult.Failed("...") : StartupCheckResult.Success;
        }
    }
}