namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using NServiceBus.AzureServiceBus.Addressing;
    using NServiceBus.ObjectBuilder.Common;
    using NServiceBus.Settings;

    // a topology is responsible to determine what the underlying physical topology in asb looks like
    // This includes, which namespaces are to be used
    // which shared resources must exist (like audit & error queue, or shared topics, eventhubs, etc...)
    // which endpoint specific resources (like input queue)
    // how these entities relate (forwarding f.e.)

    // internally it relies on addressing strategy and friends, but ultimately it provides a topology definition

    public interface ITopology
    {
        /// <summary>
        /// Properly initializes configuration, called while settings can still be changed
        /// </summary>
        void InitializeSettings();

        /// <summary>
        /// Properly sets up the container, called when settings are set
        /// </summary>
        void InitializeContainer();

        /// <summary>
        /// Creates the topology definition, called when settings are set
        /// </summary>
        TopologyDefinition Determine(Purpose purpose);

        //void Subscribe(Type eventtype) // probably need this separatly as subscriptions can be added at runtime
        //void Unsubscribe(Type eventtype) // probably need this separatly as subscriptions can be removed at runtime

    }

    // responsible for creating (part of) the topology
    // part of = only the part that matters to this endpoint

    // note there is some creation logic elsewhere already, those calls should be removed and centralized here

    public interface ICreateTopology
    {
        Task Create(ITopology topology);
    }

    // Operational aspects of running on top of the topology
    // Takes care of the topology and it's specific state at runtime
    // Examples
    // Decissions of currently active namespace go here f.e.
    // So is the list of notifiers etc...
    // etc..

    public interface IOperateTopology
    {
        Task Start();

        Task Stop();
    }

    // the classes below will hold the metadata about the topology, 
    // maybe better to pass some fo these around into the lower level infrastructure instead of regular strings

    public class TopologyDefinition
    {
        public NamespaceInfo[] Namespaces { get; set; }
        public EntityInfo[] Entities { get; set; }
        public EntityRelationShipInfo[] Relationships { get; set; }
    }

    public class NamespaceInfo
    {
        public NamespaceInfo(string connectionString, NamespaceMode mode)
        {
            ConnectionString = connectionString;
            Mode = mode;
        }

        public string ConnectionString { get; set; }

        public NamespaceMode Mode { get; set; }

        protected bool Equals(NamespaceInfo other)
        {
            return string.Equals(ConnectionString, other.ConnectionString); // && Mode == other.Mode; // namespaces can switch mode, so should not be included in the equality check
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((NamespaceInfo)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((ConnectionString != null ? ConnectionString.GetHashCode() : 0) * 397); // ^ (int)Mode; // namespaces can switch mode, so should not be included in the equality check
            }
        }

        public static bool operator ==(NamespaceInfo left, NamespaceInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(NamespaceInfo left, NamespaceInfo right)
        {
            return !(left == right);
        }
    }

    public enum NamespaceMode
    {
        Active,
        Passive
    }

    public class EntityInfo
    {
        public string Path { get; set; }

        public EntityType Type { get; set; }

        public NamespaceInfo Namespace { get; set; }

        protected bool Equals(EntityInfo other)
        {
            return string.Equals(Path, other.Path) && Type == other.Type && Equals(Namespace, other.Namespace);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((EntityInfo) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Path != null ? Path.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (int) Type;
                hashCode = (hashCode*397) ^ (Namespace != null ? Namespace.GetHashCode() : 0);
                return hashCode;
            }
        }

        public static bool operator ==(EntityInfo left, EntityInfo right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EntityInfo left, EntityInfo right)
        {
            return !Equals(left, right);
        }
    }

    public enum EntityType
    {
        Queue,
        Topic,
        Subscription,
        EventHub
    }

    public class EntityRelationShipInfo
    {
        public EntityInfo Source { get; set; }
        public EntityInfo Target { get; set; }
        public EntityRelationShipType Type { get; set; }
    }

    public enum EntityRelationShipType
    {
        Forward,
        Subscription
    }

    public class EachEndpointHasQueueAndTopic : ITopology
    {
        readonly SettingsHolder settings;
        readonly IContainer container;

        public EachEndpointHasQueueAndTopic(SettingsHolder settings, IContainer container)
        {
            this.settings = settings;
            this.container = container;
        }

        public void InitializeSettings()
        {
            // ensures settings are present/correct
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Strategy, typeof(OriginalAddressingStrategy));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Composition.Strategy, typeof(FlatCompositionStrategy));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Individualization.Strategy, typeof(DiscriminatorBasedIndividualizationStrategy));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Strategy, typeof(SingleNamespacePartitioningStrategy));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Strategy, typeof(AdjustmentSanitizationStrategy));
            settings.SetDefault(WellKnownConfigurationKeys.Topology.Addressing.Validation.Strategy, typeof(EntityNameValidationRules));
        }

        public void InitializeContainer()
        {
            // configures container
            var addressingStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Strategy);
            container.Configure(addressingStrategyType, DependencyLifecycle.InstancePerCall);

            var compositionStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Composition.Strategy);
            container.Configure(compositionStrategyType, DependencyLifecycle.InstancePerCall);

            var individualizationStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Individualization.Strategy);
            container.Configure(individualizationStrategyType, DependencyLifecycle.InstancePerCall);

            var partitioningStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Strategy);
            container.Configure(partitioningStrategyType, DependencyLifecycle.InstancePerCall);

            var sanitizationStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Sanitization.Strategy);
            container.Configure(sanitizationStrategyType, DependencyLifecycle.InstancePerCall);

            var validationStrategyType = (Type)settings.Get(WellKnownConfigurationKeys.Topology.Addressing.Validation.Strategy);
            container.Configure(validationStrategyType, DependencyLifecycle.InstancePerCall);
        }

        public TopologyDefinition Determine(Purpose purpose)
        {
            // computes the topology

            var endpointName = settings.Get<EndpointName>();

            var partitioningStrategy = (INamespacePartitioningStrategy)container.Build(typeof(INamespacePartitioningStrategy));
            var sanitazationStrategy = (ISanitizationStrategy)container.Build(typeof(ISanitizationStrategy));

            var namespaces = partitioningStrategy.GetNamespaces(endpointName.ToString(), purpose).ToArray();

            var inputqueuepath = sanitazationStrategy.Sanitize(endpointName.ToString(), Addressing.EntityType.Queue);
            var inputqueues = namespaces.Select(n => new EntityInfo { Path = inputqueuepath, Type = EntityType.Queue, Namespace = n }).ToArray();

            var topicpath = sanitazationStrategy.Sanitize(endpointName + ".events", Addressing.EntityType.Topic);
            var topics = namespaces.Select(n => new EntityInfo { Path = topicpath, Type = EntityType.Topic, Namespace = n }).ToArray();

            var entities = inputqueues.Concat(topics).ToArray();

            return new TopologyDefinition()
            {
                Namespaces = namespaces,
                Entities = entities
            };
        }

    }

    public class OriginalAddressingStrategy : IAddressingStrategy
    {
        public EntityInfo[] GetEntitiesForPublishing(Type eventType)
        {
            throw new NotImplementedException();
        }

        public EntityInfo[] GetEntitiesForSending(string destination)
        {
            throw new NotImplementedException();
        }
    }
}
