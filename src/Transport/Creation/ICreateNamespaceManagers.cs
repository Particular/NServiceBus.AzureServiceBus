namespace NServiceBus.Transport.AzureServiceBus
{
    interface ICreateNamespaceManagersInternal
    {
        INamespaceManagerInternal Create(string namespaceName);
    }
}