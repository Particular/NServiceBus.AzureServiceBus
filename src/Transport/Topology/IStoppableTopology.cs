namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Threading.Tasks;

    public interface IStoppableTopology
    {
        Task Stop();
    }
}