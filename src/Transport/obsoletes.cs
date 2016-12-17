namespace NServiceBus
{
    using System;
    using System.Reflection;
    using Microsoft.ServiceBus.Messaging;
    using Settings;
    using Transport.AzureServiceBus;

    static class ObsoleteMessages
    {
        public const string InternalizedContract = "Internal contract.";
        public const string ReplaceWithNewAPI = "Replaced with new API.";
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class ForwardingTopology : ITopology { }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class EndpointOrientedTopology : ITopology { }

    public static partial class AzureServiceBusTransportExtensions
    {
        [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public static void UseOutgoingMessageToBrokeredMessageConverter<T>(this TransportExtensions<AzureServiceBusTransport> transportExtensions) where T : IConvertOutgoingMessagesToBrokeredMessages
        {
            throw new NotImplementedException();
        }

        [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public static void UseBrokeredMessageToIncomingMessageConverter<T>(this TransportExtensions<AzureServiceBusTransport> transportExtensions) where T : IConvertBrokeredMessagesToIncomingMessages
        {
            throw new NotImplementedException();
        }

        [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0", ReplacementTypeOrMember = "transport.UseForwardingTopology() or transport.UseEndpointOrientedTopology()")]
        public static AzureServiceBusTopologySettings<T> UseTopology<T>(this TransportExtensions<AzureServiceBusTransport> transportExtensions) where T : ITopology, new()
        {
            throw new NotImplementedException();
        }

        [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0", ReplacementTypeOrMember = "transport.UseForwardingTopology() or transport.UseEndpointOrientedTopology()")]
        public static AzureServiceBusTopologySettings<T> UseTopology<T>(this TransportExtensions<AzureServiceBusTransport> transportExtensions, Func<T> factory) where T : ITopology
        {
            throw new NotImplementedException();
        }

        [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0", ReplacementTypeOrMember = "transport.UseForwardingTopology() or transport.UseEndpointOrientedTopology()")]
        public static AzureServiceBusTopologySettings<T> UseTopology<T>(this TransportExtensions<AzureServiceBusTransport> transportExtensions, T topology) where T : ITopology
        {
            throw new NotImplementedException();
        }
    }

    public static partial class AzureServiceBusEndpointOrientedTopologySettingsExtensions
    {
        [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public static AzureServiceBusTopologySettings<EndpointOrientedTopology> RegisterPublisher(this AzureServiceBusTopologySettings<EndpointOrientedTopology> topologySettings, Type type, string publisherName)
        {
            throw new NotImplementedException();
        }

        [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public static AzureServiceBusTopologySettings<EndpointOrientedTopology> RegisterPublisher(this AzureServiceBusTopologySettings<EndpointOrientedTopology> topologySettings, Assembly assembly, string publisherName)
        {
            throw new NotImplementedException();
        }
    }

    public static partial class AzureServiceBusForwardingTopologySettingsExtensions
    {
        [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public static AzureServiceBusTopologySettings<ForwardingTopology> NumberOfEntitiesInBundle(this AzureServiceBusTopologySettings<ForwardingTopology> topologySettings, int number)
        {
            throw new NotImplementedException();
        }

        [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public static AzureServiceBusTopologySettings<ForwardingTopology> BundlePrefix(this AzureServiceBusTopologySettings<ForwardingTopology> topologySettings, string prefix)
        {
            throw new NotImplementedException();
        }
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class AzureServiceBusTopologySettings<T> : TransportExtensions<AzureServiceBusTransport> where T : ITopology
    {
        public AzureServiceBusTopologySettings(SettingsHolder settings) : base(settings)
        {
        }
    }

    public partial class AzureServiceBusQueueSettings
    {
        [ObsoleteEx(Message = ObsoleteMessages.ReplaceWithNewAPI, ReplacementTypeOrMember = "DescriptionCustomizer(Action<QueueDescription>)", TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public AzureServiceBusQueueSettings DescriptionFactory(Func<string, string, ReadOnlySettings, QueueDescription> factory)
        {
            throw new NotImplementedException();
        }
    }

    public partial class AzureServiceBusTopicSettings
    {
        [ObsoleteEx(Message = ObsoleteMessages.ReplaceWithNewAPI, ReplacementTypeOrMember = "DescriptionCustomizer(Action<TopicDescription>)", TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public AzureServiceBusTopicSettings DescriptionFactory(Func<string, string, ReadOnlySettings, TopicDescription> factory)
        {
            throw new NotImplementedException();
        }
    }

    public partial class AzureServiceBusSubscriptionSettings
    {
        [ObsoleteEx(Message = ObsoleteMessages.ReplaceWithNewAPI, ReplacementTypeOrMember = "DescriptionCustomizer(Action<SubscriptionDescription>)", TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public AzureServiceBusSubscriptionSettings DescriptionFactory(Func<string, string, ReadOnlySettings, SubscriptionDescription> factory)
        {
            throw new NotImplementedException();
        }
    }
}

namespace NServiceBus.Transport.AzureServiceBus
{
    using System;

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface ITopology
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface INamespaceManager
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface ICreateAzureServiceBusQueues
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface ICreateAzureServiceBusTopics
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IManageNamespaceManagerLifeCycle
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface ICreateAzureServiceBusSubscriptions
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface ICreateNamespaceManagers
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface ITopologySectionManager
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface ICreateMessagingFactories
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IManageMessagingFactoryLifeCycle
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface ICreateMessageSenders
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IBatcher
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IConvertOutgoingMessagesToBrokeredMessages
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IConvertBrokeredMessagesToIncomingMessages
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IClientEntity
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IManageMessageReceiverLifeCycle
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IMessageReceiver : IClientEntity
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IMessagingFactory : IClientEntity
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IMessageSender : IClientEntity
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IManageMessageSenderLifeCycle
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface INotifyIncomingMessages
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IRouteOutgoingBatches
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface ICreateTopology
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface ICreateMessageReceivers
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IOperateTopology
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IRegisterTransportParts
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IResolveTransportParts
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface ITransportPartsContainer : IRegisterTransportParts, IResolveTransportParts { }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IBrokerSideSubscriptionFilter
    {
    }

    [ObsoleteEx(Message = "Internal unutilized contract that shouldn't be exposed.", TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IClientSideSubscriptionFilter
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class RoutingOptions
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class SubscriptionMetadata
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class NamespaceManagerAdapter : INamespaceManager
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class EntityInfo
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class IncomingMessageDetails
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public abstract class ReceiveContext
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class BrokeredMessageReceiveContext : ReceiveContext
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class EntityRelationShipInfo
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public enum EntityRelationShipType
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class TopologySection
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class Batch
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class BatchedOperation
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class SubscriptionInfo : EntityInfo
    {
    }

    [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public class ConnectionString
    {
    }

    public partial class NamespaceInfo
    {
        [ObsoleteEx(Message = ObsoleteMessages.InternalizedContract, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
        public ConnectionString ConnectionString
        {
            get {  throw new NotImplementedException(); }
        }
    }
}
