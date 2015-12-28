namespace NServiceBus.AzureServiceBus
{
    using System.Threading.Tasks;

    public interface IManageMessagingFactoryLifeCycle
    {
        IMessagingFactory Get(string @namespace);

        Task CloseAll();
    }
}