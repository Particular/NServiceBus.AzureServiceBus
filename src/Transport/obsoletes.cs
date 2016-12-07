namespace NServiceBus
{
    using System;
    using Transport.AzureServiceBus;

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class ForwardingTopology : ITopology { }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class EndpointOrientedTopology : ITopology { }

    public static partial class AzureServiceBusTransportExtensions
    {
        [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public static void UseOutgoingMessageToBrokeredMessageConverter<T>(this TransportExtensions<AzureServiceBusTransport> transportExtensions) where T : IConvertOutgoingMessagesToBrokeredMessages
        {
            throw new NotImplementedException();
        }

        [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public static void UseBrokeredMessageToIncomingMessageConverter<T>(this TransportExtensions<AzureServiceBusTransport> transportExtensions) where T : IConvertBrokeredMessagesToIncomingMessages
        {
            throw new NotImplementedException();
        }

        [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0", ReplacementTypeOrMember = "transport.UseForwardingTopology() or transport.UseEndpointOrientedTopology()")]
        public static AzureServiceBusTopologySettings<T> UseTopology<T>(this TransportExtensions<AzureServiceBusTransport> transportExtensions) where T : ITopology, new()
        {
            throw new NotImplementedException();
        }

        [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0", ReplacementTypeOrMember = "transport.UseForwardingTopology() or transport.UseEndpointOrientedTopology()")]
        public static AzureServiceBusTopologySettings<T> UseTopology<T>(this TransportExtensions<AzureServiceBusTransport> transportExtensions, Func<T> factory) where T : ITopology
        {
            throw new NotImplementedException();
        }

        [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0", ReplacementTypeOrMember = "transport.UseForwardingTopology() or transport.UseEndpointOrientedTopology()")]
        public static AzureServiceBusTopologySettings<T> UseTopology<T>(this TransportExtensions<AzureServiceBusTransport> transportExtensions, T topology) where T : ITopology
        {
            throw new NotImplementedException();
        }
    }
}

namespace NServiceBus.Transport.AzureServiceBus
{
    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface ITopology
    {
    }

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
    public interface ICreateAzureServiceBusSubscriptions
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface ICreateNamespaceManagers
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface ITopologySectionManager
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface ICreateMessagingFactories
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IManageMessagingFactoryLifeCycle
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface ICreateMessageSenders
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IBatcher
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IConvertOutgoingMessagesToBrokeredMessages
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IConvertBrokeredMessagesToIncomingMessages
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IClientEntity
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IManageMessageReceiverLifeCycle
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IMessageReceiver : IClientEntity
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IMessagingFactory : IClientEntity
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IMessageSender : IClientEntity
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface INotifyIncomingMessages
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IRouteOutgoingBatches
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class NamespaceManagerAdapter : INamespaceManager
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class EntityInfo
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class IncomingMessageDetails
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public abstract class ReceiveContext
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class BrokeredMessageReceiveContext : ReceiveContext
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class EntityRelationShipInfo
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public enum EntityRelationShipType
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class TopologySection
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class Batch
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class BatchedOperation
    {
    }
}
