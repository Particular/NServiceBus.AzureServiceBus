namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;

    // For now let's make this an internal interface in order to be able to hotfix the shutdown problem
    interface IStoppableTopology
    {
        Task Stop();
    }
}