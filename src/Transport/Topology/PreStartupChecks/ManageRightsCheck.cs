namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Settings;
    using Transport;

    class ManageRightsCheck
    {
        IManageNamespaceManagerLifeCycle manageNamespaceManagerLifeCycle;
        ReadOnlySettings settings;

        public ManageRightsCheck(IManageNamespaceManagerLifeCycle manageNamespaceManagerLifeCycle, ReadOnlySettings settings)
        {
            this.manageNamespaceManagerLifeCycle = manageNamespaceManagerLifeCycle;
            this.settings = settings;
        }

        public async Task<StartupCheckResult> Run()
        {
            if (!settings.Get<bool>(WellKnownConfigurationKeys.Core.CreateTopology))
            {
                return StartupCheckResult.Success;
            }

            var namespacesWithoutManageRights = new List<string>();

            var namespaces = settings.Get<NamespaceConfigurations>(WellKnownConfigurationKeys.Topology.Addressing.Namespaces);
            foreach (var @namespace in namespaces)
            {
                var namespaceManager = manageNamespaceManagerLifeCycle.Get(@namespace.Alias);
                var canManageEntities = await namespaceManager.CanManageEntities().ConfigureAwait(false);

                if (!canManageEntities)
                {
                    namespacesWithoutManageRights.Add(@namespace.Alias);
                }
            }

            if (namespacesWithoutManageRights.Any() == false)
            {
                return StartupCheckResult.Success;
            }

            return StartupCheckResult.Failed($"Configured to create topology, but have no manage rights for the following namespace(s): {string.Join(", ", namespacesWithoutManageRights.Select(alias => $"`{alias}`"))}.");
        }
    }
}