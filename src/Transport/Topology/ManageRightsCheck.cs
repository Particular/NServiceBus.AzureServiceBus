namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Settings;

    static class ManageRightsCheck
    {
        public static async Task<List<string>> Run(IManageNamespaceManagerLifeCycle manageNamespaceManagerLifeCycle, ReadOnlySettings settings)
        {
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

            return namespacesWithoutManageRights;
        }
    }
}