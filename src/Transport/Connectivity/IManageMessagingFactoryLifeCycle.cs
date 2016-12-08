namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;

    interface IManageMessagingFactoryLifeCycleInternal
    {
        IMessagingFactoryInternal Get(string namespaceName);

        Task CloseAll();
    }
}