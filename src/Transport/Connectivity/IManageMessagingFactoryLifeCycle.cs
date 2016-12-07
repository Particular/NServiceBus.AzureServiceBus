namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;

    interface IManageMessagingFactoryLifeCycleInternal
    {
        IMessagingFactory Get(string namespaceName);

        Task CloseAll();
    }
}