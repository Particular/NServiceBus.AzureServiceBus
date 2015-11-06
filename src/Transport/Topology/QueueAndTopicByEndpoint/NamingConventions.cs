namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.QueueAndTopicByEndpoint
{
    using System;
    using System.Text.RegularExpressions;
    using Config;
    using Settings;

    public static class NamingConventions
    {
        public static Func<ReadOnlySettings, Type, string, bool, string> QueueNamingConvention
        {
            get { return defaultQueueNamingConvention; }
            set { defaultQueueNamingConvention = value; }
        }

        static Func<ReadOnlySettings, Type, string, bool, string> defaultQueueNamingConvention = (settings, messagetype, queueName, doNotIndividualize) =>
        {
            var configSection = settings != null ? settings.GetConfigSection<AzureServiceBusQueueConfig>() : null;

            queueName = SanitizeEntityName(queueName, EntityType.Queue);

            if (queueName.Length >= 283) // 290 - a spot for the "-" & 6 digits for the individualizer
                queueName = new DeterministicGuidBuilder().Build(queueName).ToString();

            if (!doNotIndividualize && ShouldIndividualize(configSection, settings))
                queueName = QueueIndividualizer.Individualize(queueName);

            return queueName;
        };

        static string SanitizeEntityName(string queueName, EntityType entityType)
        {
            return EntitySanitizationConvention(queueName, entityType);
        }

        public static Func<string, EntityType, string> EntitySanitizationConvention 
        {
            get { return defaultEntitySanitizationConvention;  }
            set { defaultEntitySanitizationConvention = value; } 
        }

        static Func<string, EntityType, string> defaultEntitySanitizationConvention = (name, entityPath) =>
        {
            // Entity segments can contain only letters, numbers, periods (.), hyphens (-), and underscores (_)
            var rgx = new Regex(@"[^a-zA-Z0-9\-._]");
            return rgx.Replace(name, "");
        };

        static bool ShouldIndividualize(AzureServiceBusQueueConfig configSection, ReadOnlySettings settings)
        {
            // if this setting is set, then the core is responsible for individualization
            if (settings != null && settings.HasExplicitValue("IndividualizeEndpointAddress"))
            {
                return false;
            }

            // if explicitly set in code
            if (settings != null && settings.HasExplicitValue("ScaleOut.UseSingleBrokerQueue"))
                return !settings.Get<bool>("ScaleOut.UseSingleBrokerQueue");

            // if explicitly set in config
            if (configSection != null)
                return configSection.QueuePerInstance;

            // if default is set
            if (settings != null && !settings.GetOrDefault<bool>("ScaleOut.UseSingleBrokerQueue"))
                return !settings.GetOrDefault<bool>("ScaleOut.UseSingleBrokerQueue");

            return false;
        }

        public static Func<ReadOnlySettings, Type, string, string> SubscriptionNamingConvention
        {
            get { return defaultSubscriptionNamingConvention; }
            set { defaultSubscriptionNamingConvention = value; }
        }

        private static Func<ReadOnlySettings, Type, string, string> defaultSubscriptionNamingConvention = (settings, eventType, endpointName) =>
        {
            return BuildSubscriptionName(settings, endpointName, eventType, e => e.Name);
        };

        public static Func<ReadOnlySettings, Type, string, string> SubscriptionFullNamingConvention
        {
            get { return defaultSubscriptionFullNamingConvention; }
            set { defaultSubscriptionFullNamingConvention = value; }
        }

        private static Func<ReadOnlySettings, Type, string, string> defaultSubscriptionFullNamingConvention = (settings, eventType, endpointName) =>
        {
            return BuildSubscriptionName(settings, endpointName, eventType, e => e.FullName);
        };

        private static string BuildSubscriptionName(ReadOnlySettings settings, string endpointName, Type eventType, Func<Type, string> eventTypeNameBuilder)
        {
            var subscriptionName = eventType != null ? endpointName + "." + eventTypeNameBuilder(eventType) : endpointName;

            subscriptionName = SanitizeEntityName(subscriptionName, EntityType.Subscription);

            if (subscriptionName.Length >= 50)
                subscriptionName = new DeterministicGuidBuilder().Build(subscriptionName).ToString();

            if (ShouldIndividualize(null, settings))
            {
                subscriptionName = QueueIndividualizer.Individualize(subscriptionName);

                // check length again in case individualization made it too long
                if (subscriptionName.Length >= 50)
                    subscriptionName = new DeterministicGuidBuilder().Build(subscriptionName).ToString();
            }

            return subscriptionName; 
        }

        public static Func<ReadOnlySettings, Type, string, string> TopicNamingConvention
        {
            get { return defaultTopicNamingConvention; }
            set { defaultTopicNamingConvention = value; }
        }

        private static Func<ReadOnlySettings, Type, string, string> defaultTopicNamingConvention = (settings, messagetype, endpointname) =>
        {
            var name = endpointname;

            name = SanitizeEntityName(name, EntityType.Topic);

            if (name.Length >= 290)
                name = new DeterministicGuidBuilder().Build(name).ToString();

            return name;
        };

        public static Func<ReadOnlySettings, Address, Address> PublisherAddressConvention
        {
            get { return defaultPublisherAddressConvention; }
            set { defaultPublisherAddressConvention = value; }
        }

        private static Func<ReadOnlySettings, Address, Address> defaultPublisherAddressConvention = (settings, address) => Address.Parse(TopicNamingConvention(settings, null, address.Queue + ".events") + "@" + address.Machine);

        public static Func<ReadOnlySettings, Address, Address> PublisherAddressConventionForSubscriptions
        {
            get { return PublisherAddressConvention; }
            set { PublisherAddressConvention = value; }
        }

        public static Func<ReadOnlySettings, Address, bool, Address> QueueAddressConvention
        {
            get { return defaultQueueAddressConvention; }
            set { defaultQueueAddressConvention = value; }
        }

        private static Func<ReadOnlySettings, Address, bool, Address> defaultQueueAddressConvention = (settings, address, doNotIndividualize) => Address.Parse(QueueNamingConvention(settings, null, address.Queue, doNotIndividualize) + "@" + address.Machine);
    }

         public enum EntityType
        {
            Queue,
            Topic,
            Subscription
        }

}