namespace NServiceBus.AzureServiceBus.Topology.MetaModel
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Transport.AzureServiceBus;

    static class ManageRightsCheck
    {
        public static async Task<List<string>> Run(IManageNamespaceManagerLifeCycleInternal manageNamespaceManagerLifeCycle, NamespaceConfigurations namespaceConfigurations)
        {
            var namespacesWithoutManageRights = new List<string>();

            foreach (var @namespace in namespaceConfigurations)
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