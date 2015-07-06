namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus.QueueAndTopicByEndpoint
{
    using System;
    using System.Text.RegularExpressions;
    using Config;
    using Settings;

    static class NamingConventions
    {
        internal static Func<ReadOnlySettings, Type, string, bool, string> QueueNamingConvention
        {
            get
            {
                return (settings, messagetype, queueName, doNotIndividualize) =>
                {
                    var configSection = settings != null ? settings.GetConfigSection<AzureServiceBusQueueConfig>() : null;

                    queueName = SanitizeEntityName(queueName);

                    if (queueName.Length >= 283) // 290 - a spot for the "-" & 6 digits for the individualizer
                        queueName = new DeterministicGuidBuilder().Build(queueName).ToString();

                    if (!doNotIndividualize && ShouldIndividualize(configSection, settings))
                        queueName = QueueIndividualizer.Individualize(queueName);

                    return queueName;
                };
            }
        }

        static string SanitizeEntityName(string queueName)
        {
            //*Entity segments can contain only letters, numbers, periods (.), hyphens (-), and underscores */

            var rgx = new Regex(@"[^a-zA-Z0-9\-._]");
            var n = rgx.Replace(queueName, "");
            return n;
        }

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

        internal static Func<ReadOnlySettings, Type, string, string> SubscriptionNamingConvention
        {
            get
            {
                return (settings, eventType, endpointName) =>
                {
                    return BuildSubscriptionName(settings, endpointName, eventType, e => e.Name);
                };
            }
        }

        internal static Func<ReadOnlySettings, Type, string, string> SubscriptionFullNamingConvention
        {
            get
            {
                return (settings, eventType, endpointName) =>
                {
                    return BuildSubscriptionName(settings, endpointName, eventType, e => e.FullName);
                };
            }
        }

        private static string BuildSubscriptionName(ReadOnlySettings settings, string endpointName, Type eventType, Func<Type, string> eventTypeNameBuilder)
        {
            var subscriptionName = eventType != null ? endpointName + "." + eventTypeNameBuilder(eventType) : endpointName;

            subscriptionName = SanitizeEntityName(subscriptionName);

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

        internal static Func<ReadOnlySettings, Type, string, string> TopicNamingConvention
        {
            get
            {
                return (settings, messagetype, endpointname) =>
                {
                    var name = endpointname;

                    name = SanitizeEntityName(name);

                    if (name.Length >= 290)
                        name = new DeterministicGuidBuilder().Build(name).ToString();

                    return name;
                };
            }
        }

        internal static Func<ReadOnlySettings, Address, Address> PublisherAddressConvention
        {
            get
            {
                return (settings, address) => Address.Parse(TopicNamingConvention(settings, null, address.Queue + ".events") + "@" + address.Machine);
            }
        }

        internal static Func<ReadOnlySettings, Address, Address> PublisherAddressConventionForSubscriptions
        {
            get { return PublisherAddressConvention; }
        }

        internal static Func<ReadOnlySettings, Address, bool, Address> QueueAddressConvention
        {
            get
            {
                return (settings, address, doNotIndividualize) => Address.Parse(QueueNamingConvention(settings, null, address.Queue, doNotIndividualize) + "@" + address.Machine);
            }
        }
    }
}