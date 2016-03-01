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

        public ManageRightsCheck(IManageNamespaceManagerLifeCycle manageNamespaceManagerLifeCycle, ReadOnlySettings settings)
        {
            this.manageNamespaceManagerLifeCycle = manageNamespaceManagerLifeCycle;
            this.settings = settings;
        }

        public async Task<StartupCheckResult> Run()
        {
            if (!settings.Get<bool>(WellKnownConfigurationKeys.Core.CreateTopology))
                return StartupCheckResult.Success;

            var namespacesWithoutManageRights = new List<string>();

            var namespaces = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces);
            foreach (var @namespace in namespaces)
            {
                var namespaceManager = manageNamespaceManagerLifeCycle.Get(@namespace.Name);
                var canManageEntities = await namespaceManager.CanManageEntities().ConfigureAwait(false);

                if (!canManageEntities)
                    namespacesWithoutManageRights.Add(@namespace.Name);
            }

            if (namespacesWithoutManageRights.Any() == false)
                return StartupCheckResult.Success;

            return StartupCheckResult.Failed($"Configured to create topology, but have no manage rights for the following namespace(s): {string.Join(", ", namespacesWithoutManageRights.Select(name => $"`{name}`"))}.");
        }
    }
}