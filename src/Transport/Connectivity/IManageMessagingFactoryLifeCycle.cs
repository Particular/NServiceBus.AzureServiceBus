namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;

    public interface IManageMessagingFactoryLifeCycle
    {
        IMessagingFactory Get(string namespaceName);

        Task CloseAll();
    }
}