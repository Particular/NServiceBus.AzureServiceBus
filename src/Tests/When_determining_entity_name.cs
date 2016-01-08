namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests
{
    using System.Text.RegularExpressions;
    using NServiceBus.Azure.Transports.WindowsAzureServiceBus.QueueAndTopicByEndpoint;
    using NUnit.Framework;

    [TestFixture]
    public class When_determining_entity_path
    {
        [TestCase("queue", "queue")]
        [TestCase("queue1234", "queue1234")]
        [TestCase("1234queue", "1234queue")]
        [TestCase("queue.1", "queue.1")]
        [TestCase("queue-1", "queue-1")]
        [TestCase("queue/with/segments", "queuewithsegments")] // for backwards compatibility we need to strip inner "/" as well, but those are proper for vNext!
        [TestCase("/queue/segmented", "queuesegmented")] // for backwards compatibility we need to strip inner "/" as well, but those are proper for vNext!
        [TestCase("queue/segmented/", "queuesegmented")] // for backwards compatibility we need to strip inner "/" as well, but those are proper for vNext!
        [TestCase("/queue/segmented/", "queuesegmented")] // for backwards compatibility we need to strip inner "/" as well, but those are proper for vNext!
        public void Should_remove_invalid_charachters_for_queue_path(string queuePath, string expectedName)
        {
            var queueName = NamingConventions.QueueNamingConvention(null, null, queuePath, false);
            Assert.AreEqual(expectedName, queueName);
        }

        [TestCase("topic", "topic")]
        [TestCase("topic1234", "topic1234")]
        [TestCase("1234topic", "1234topic")]
        [TestCase("topic.1", "topic.1")]
        [TestCase("topic-1", "topic-1")]
        [TestCase("topic/with/segments", "topicwithsegments")] // for backwards compatibility we need to strip inner "/" as well, but those are proper for vNext!
        [TestCase("/topic/segmented", "topicsegmented")] // for backwards compatibility we need to strip inner "/" as well, but those are proper for vNext!
        [TestCase("topic/segmented/", "topicsegmented")] // for backwards compatibility we need to strip inner "/" as well, but those are proper for vNext!
        [TestCase("/topic/segmented/", "topicsegmented")] // for backwards compatibility we need to strip inner "/" as well, but those are proper for vNext!
        public void Should_remove_invalid_charachters_for_topic_path(string topicPath, string expectedName)
        {
            var topicName = NamingConventions.TopicNamingConvention(null, null, topicPath);
            Assert.AreEqual(expectedName, topicName);
        }

        [TestCase("subscription", "subscription")]
        [TestCase("subscription1234", "subscription1234")]
        [TestCase("1234subscription", "1234subscription")]
        [TestCase("subscription.1", "subscription.1")]
        [TestCase("subscription-1", "subscription-1")]
        [TestCase("subscription/with/segments", "subscriptionwithsegments")]
        [TestCase("/subscription/segmented", "subscriptionsegmented")]
        [TestCase("subscription/segmented/", "subscriptionsegmented")]
        [TestCase("/subscription/segmented/", "subscriptionsegmented")]
        public void Should_remove_invalid_charachters_for_subscription_path(string subscriptionPath, string expectedName)
        {
            var subscriptionName = NamingConventions.SubscriptionNamingConvention(null, null, subscriptionPath);
            Assert.AreEqual(expectedName, subscriptionName);
        }

        [TestCase("queue/with/segments", "queue/with/segments")]
        [TestCase("/queue/segmented", "queue/segmented")]
        [TestCase("queue/segmented/", "queue/segmented")]
        [TestCase("/queue/segmented/", "queue/segmented")]
        public void Should_be_able_to_plug_custom_sanitization_convention_to_remove_invalid_charachters_for_queue_path(string queuePath, string expectedName)
        {
            var savedConvention = NamingConventions.EntitySanitizationConvention;
            NamingConventions.EntitySanitizationConvention = CustomSanitizationConvention;

            var queueName = NamingConventions.QueueNamingConvention(null, null, queuePath, false);
            
            NamingConventions.EntitySanitizationConvention = savedConvention;
            
            Assert.AreEqual(expectedName, queueName);
        }

        private static string CustomSanitizationConvention(string name, EntityType entityType)
        {
            if (entityType == EntityType.Queue || entityType == EntityType.Topic)
            {
                var regexQueueAndTopicValidCharacters = new Regex(@"[^a-zA-Z0-9\-\._\/]");
                var regexLeadingAndTrailingForwardSlashes = new Regex(@"^\/|\/$");

                var result = regexQueueAndTopicValidCharacters.Replace(name, "");
                return regexLeadingAndTrailingForwardSlashes.Replace(result, "");
            }

            // subscription
            var regexSubscriptionValidCharacters = new Regex(@"[^a-zA-Z0-9\-\._]");
            return regexSubscriptionValidCharacters.Replace(name, "");
        }
    }
}