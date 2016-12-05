namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;

    interface IStoppableTopology
    {
        Task Stop();
    }
}