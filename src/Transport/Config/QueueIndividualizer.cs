namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using System.Globalization;
    using Support;

    class QueueIndividualizer
    {
        public static string Individualize(string queueName)
        {
            var parser = new ConnectionStringParser();
            var individualQueueName = queueName;
            if (SafeRoleEnvironment.IsAvailable)
            {
                var index = parser.ParseIndexFrom(SafeRoleEnvironment.CurrentRoleInstanceId);

                var currentQueue = parser.ParseQueueNameFrom(queueName);
                if (!currentQueue.Contains("-" + index.ToString(CultureInfo.InvariantCulture))) //individualize can be applied multiple times, should exlude subqueues
                {
                    individualQueueName = currentQueue
                                          + (index > 0 ? "-" : "")
                                          + (index > 0 ? index.ToString(CultureInfo.InvariantCulture) : "");

                    if (queueName.Contains("@"))
                        individualQueueName += "@" + parser.ParseNamespaceFrom(queueName);
                }
            }
            else
            {
                var currentQueue = parser.ParseQueueNameFrom(queueName);
                if (!currentQueue.Contains("-" + RuntimeEnvironment.MachineName)) //individualize can be applied multiple times, should exlude subqueues
                {
                    individualQueueName = currentQueue + "-" + RuntimeEnvironment.MachineName;

                    if (queueName.Contains("@"))
                        individualQueueName += "@" + parser.ParseNamespaceFrom(queueName);
                }
            }

            return individualQueueName;
        }

        public static string Discriminator {
            get
            {
                if (SafeRoleEnvironment.IsAvailable)
                {
                    var parser = new ConnectionStringParser();
                    var index = parser.ParseIndexFrom(SafeRoleEnvironment.CurrentRoleInstanceId);
                    return "-" + index.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    return "-" + RuntimeEnvironment.MachineName;
                }

            }
        }
    }
}