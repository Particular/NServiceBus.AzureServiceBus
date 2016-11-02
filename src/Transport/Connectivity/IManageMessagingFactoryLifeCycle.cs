namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IManageMessagingFactoryLifeCycle
    {
        IMessagingFactory Get(string namespaceName);

        Task CloseAll();
    }
}