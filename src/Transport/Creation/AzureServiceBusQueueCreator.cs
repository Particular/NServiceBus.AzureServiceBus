namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Reflection;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NServiceBus.Logging;
    using NServiceBus.Settings;

    class AzureServiceBusQueueCreator : ICreateAzureServiceBusQueues
    {
        ConcurrentDictionary<string, bool> rememberExistence = new ConcurrentDictionary<string, bool>();
        ReadOnlySettings _settings;
        Func<string, ReadOnlySettings, QueueDescription> _descriptionFactory;

        ILog logger = LogManager.GetLogger(typeof(AzureServiceBusQueueCreator));

        public AzureServiceBusQueueCreator(ReadOnlySettings settings)
        {
            _settings = settings;

            if(!_settings.TryGet(WellKnownConfigurationKeys.Topology.Resources.Queues.DescriptionFactory, out _descriptionFactory))
            {
                _descriptionFactory = (name, s) => new QueueDescription(name)
                {
                    LockDuration = s.GetOrDefault<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Queues.LockDuration),
                    MaxSizeInMegabytes = s.GetOrDefault<long>(WellKnownConfigurationKeys.Topology.Resources.Queues.MaxSizeInMegabytes),
                    RequiresDuplicateDetection = s.GetOrDefault<bool>(WellKnownConfigurationKeys.Topology.Resources.Queues.RequiresDuplicateDetection),
                    RequiresSession = s.GetOrDefault<bool>(WellKnownConfigurationKeys.Topology.Resources.Queues.RequiresSession),
                    DefaultMessageTimeToLive = s.GetOrDefault<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Queues.DefaultMessageTimeToLive),
                    EnableDeadLetteringOnMessageExpiration = s.GetOrDefault<bool>(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableDeadLetteringOnMessageExpiration),
                    DuplicateDetectionHistoryTimeWindow = s.GetOrDefault<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Queues.DuplicateDetectionHistoryTimeWindow),
                    MaxDeliveryCount = s.GetOrDefault<int>(WellKnownConfigurationKeys.Topology.Resources.Queues.MaxDeliveryCount),
                    EnableBatchedOperations = s.GetOrDefault<bool>(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableBatchedOperations),
                    EnablePartitioning = s.GetOrDefault<bool>(WellKnownConfigurationKeys.Topology.Resources.Queues.EnablePartitioning),
                    SupportOrdering = s.GetOrDefault<bool>(WellKnownConfigurationKeys.Topology.Resources.Queues.SupportOrdering),
                    AutoDeleteOnIdle = s.GetOrDefault<TimeSpan>(WellKnownConfigurationKeys.Topology.Resources.Queues.AutoDeleteOnIdle),
                    EnableExpress = s.GetOrDefault<bool>(WellKnownConfigurationKeys.Topology.Resources.Queues.EnableExpress),
                    ForwardDeadLetteredMessagesTo = s.GetConditional<string>(name, WellKnownConfigurationKeys.Topology.Resources.Queues.ForwardDeadLetteredMessagesTo),
                    ForwardTo = s.GetConditional<string>(name, WellKnownConfigurationKeys.Topology.Resources.Queues.ForwardTo)
                };
            }
        }

        public QueueDescription Create(string queuePath, NamespaceManager namespaceManager)
        {
            var description = _descriptionFactory(queuePath, _settings);

            try
            {
                if (_settings.GetOrDefault<bool>(WellKnownConfigurationKeys.Core.CreateTopology))
                {
                    if (!Exists(namespaceManager, description.Path))
                    {
                        namespaceManager.CreateQueue(description);
                        logger.InfoFormat("Queue '{0}' created", description.Path);

                        rememberExistence.AddOrUpdate(description.Path, s => true, (s,b) => true);
                    }
                    else
                    {
                        logger.InfoFormat("Queue '{0}' already exists, skipping creation", description.Path);
                        logger.InfoFormat("Checking if queue '{0}' needs to be updated", description.Path);
                        if (!namespaceManager.GetQueue(description.Path).AllMembersAreEqual(description))
                        {
                            logger.InfoFormat("Updating queue '{0}' with new description", description.Path);
                            namespaceManager.UpdateQueue(description);
                        }
                    }
                }
                else
                {
                    logger.InfoFormat("Transport.CreateQueues is set to false, skipping the creation of '{0}'", description.Path);
                }
            }
            catch (MessagingEntityAlreadyExistsException)
            {
                // the queue already exists or another node beat us to it, which is ok
                logger.InfoFormat("Queue '{0}' already exists, another node probably beat us to it", description.Path);
            }
            catch (TimeoutException)
            {
                logger.InfoFormat("Timeout occured on queue creation for '{0}' going to validate if it doesn't exist", description.Path);

                // there is a chance that the timeout occurs, but the queue is created still
                // check for this
                if (!Exists(namespaceManager, description.Path))
                {
                    throw;
                }

                logger.InfoFormat("Looks like queue '{0}' exists anyway", description.Path);
            }
            catch (MessagingException ex)
            {
                if (!ex.IsTransient && !CreationExceptionHandling.IsCommon(ex))
                {
                    logger.Fatal(string.Format("{1} {2} occured on queue creation {0}", description.Path, (ex.IsTransient ? "Transient" : "Non transient"), ex.GetType().Name), ex);
                    throw;
                }

                logger.Info(string.Format("{1} {2} occured on queue creation {0}", description.Path, (ex.IsTransient ? "Transient" : "Non transient"), ex.GetType().Name), ex);
            }

            return description;
        }


        public bool Exists(NamespaceManager namespaceClient, string queuePath)
        {
            var key = queuePath;
            logger.InfoFormat("Checking existence cache for '{0}'", queuePath);
            var exists = rememberExistence.GetOrAdd(key, s =>
            {
                logger.InfoFormat("Checking namespace for existance of the queue '{0}'", queuePath);
                return namespaceClient.QueueExists(key);
            });

            logger.InfoFormat("Determined, from cache, that the queue '{0}' {1}", queuePath, exists ? "exists" : "does not exist");

            return exists;
        }

        

    }

    static class ReadOnlySettingsExtensions
    {
        internal static T GetConditional<T>(this ReadOnlySettings settings, string name, string key)
        {
            var condition = settings.Get<Func<string, bool>>(key + "Condition");
            return GetConditional<T>(settings, () => condition(name), key);
        }

        //todo, these 2 methods should become part of the core

        internal static T GetConditional<T>(this ReadOnlySettings settings, Func<bool> condition, string key)
        {
            if (condition())
            {
                return settings.GetOrDefault<T>(key);
            }

            return settings.GetDefault<T>(key);
        }

        internal static T GetDefault<T>(this ReadOnlySettings settings, string key)
        {
            object result;
            var bindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            var defaults = (ConcurrentDictionary<string, object>)typeof(SettingsHolder).GetField("Defaults", bindingFlags).GetValue(settings);
            if (defaults.TryGetValue(key, out result))
            {
                return (T)result;
            }

            return default(T);
        }
    }

    static class MemberComparison
    {
        public static bool AllMembersAreEqual(this object left, object right)
        {
            if (ReferenceEquals(left, right))
                return true;

            if (left == null || right == null)
                return false;

            var type = left.GetType();
            if (type != right.GetType())
                return false;

            if (left is ValueType)
            {
                // do a field comparison, or use the override if Equals is implemented:
                return left.Equals(right);
            }

            // check for override:
            if (type != typeof(object)
                && type == type.GetMethod("Equals").DeclaringType)
            {
                // the Equals method is overridden, use it:
                return left.Equals(right);
            }

            // all Arrays, Lists, IEnumerable<> etc implement IEnumerable
            if (left is IEnumerable)
            {
                var rightEnumerator = (right as IEnumerable).GetEnumerator();
                rightEnumerator.Reset();
                foreach (object leftItem in left as IEnumerable)
                {
                    // unequal amount of items
                    if (!rightEnumerator.MoveNext())
                        return false;
                    else
                    {
                        if (!AllMembersAreEqual(leftItem, rightEnumerator.Current))
                            return false;
                    }
                }
            }
            else
            {
                // compare each property
                foreach (PropertyInfo info in type.GetProperties(
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.Instance |
                    BindingFlags.GetProperty))
                {
                    // TODO: need to special-case indexable properties
                    if (!AllMembersAreEqual(info.GetValue(left, null), info.GetValue(right, null)))
                        return false;
                }

                // compare each field
                foreach (FieldInfo info in type.GetFields(
                    BindingFlags.GetField |
                    BindingFlags.NonPublic |
                    BindingFlags.Public |
                    BindingFlags.Instance))
                {
                    if (!AllMembersAreEqual(info.GetValue(left), info.GetValue(right)))
                        return false;
                }
            }
            return true;
        }
    }
}