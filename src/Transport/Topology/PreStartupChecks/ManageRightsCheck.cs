namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Linq;
    using NServiceBus.Settings;
    using NServiceBus.Transports;

    class ManageRightsCheck 
    {
        private readonly IManageNamespaceManagerLifeCycle manageNamespaceManagerLifeCycle;
        private readonly ReadOnlySettings settings;

        public ManageRightsCheck(ITransportPartsContainer container)
        {
            this.manageNamespaceManagerLifeCycle = container.Resolve<IManageNamespaceManagerLifeCycle>();
            this.settings = container.Resolve<ReadOnlySettings>();
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