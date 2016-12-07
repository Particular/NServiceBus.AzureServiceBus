// move obsoletes with error for next major here
namespace NServiceBus
{
    using Transport.AzureServiceBus;

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class ForwardingTopology : ITopology { }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class EndpointOrientedTopology : ITopology { }
}

namespace NServiceBus.Transport.AzureServiceBus
{
    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface ITopology { }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface INamespaceManager
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface ICreateAzureServiceBusQueues
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface ICreateAzureServiceBusTopics
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IManageNamespaceManagerLifeCycle
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface ICreateAzureServiceBusSubscriptions { }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface ICreateNamespaceManagers { }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class NamespaceManagerAdapter : INamespaceManager { }
}
