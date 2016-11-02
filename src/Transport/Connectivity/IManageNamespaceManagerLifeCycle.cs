namespace NServiceBus.Transport.AzureServiceBus
{
    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IManageNamespaceManagerLifeCycle
    {
        INamespaceManager Get(string namespaceAlias);
    }
}