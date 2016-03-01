namespace NServiceBus.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
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

        public async Task<StartupCheckResult> Run()
        {
            if (!settings.Get<bool>(WellKnownConfigurationKeys.Core.CreateTopology))
                return StartupCheckResult.Success;

            var namespacesWithWrongRights = new List<NamespaceInfo>();

            var namespaces = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);
            foreach (var @namespace in namespaces)
            {
                var namespaceManager = manageNamespaceManagerLifeCycle.Get(@namespace.Name);
                var canManageEntities = await namespaceManager.CanManageEntities().ConfigureAwait(false);

                if (!canManageEntities)
                    namespacesWithWrongRights.Add(@namespace);
            }

            if (!namespacesWithWrongRights.Any())
                return StartupCheckResult.Success;

            return StartupCheckResult.Failed($"Manage rights on namespace(s) is required if {WellKnownConfigurationKeys.Core.CreateTopology} setting is true." +
                                             $"Configure namespace(s) {namespacesWithWrongRights.Select(x => x.Name).Aggregate((curr, next) => string.Concat(curr, ", ", next))} with manage rights");
        }
    }
}