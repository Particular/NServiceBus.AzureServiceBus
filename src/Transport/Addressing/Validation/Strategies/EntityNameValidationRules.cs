namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using System.Text.RegularExpressions;

    public class EntityNameValidationRules : IValidationStrategy
    {
        const int QueuePathMaxLength = 260;
        const int TopicPathMaxLength = 260;
        const int SubscriptionPathMaxLength = 50;

        public bool IsValid(string entityPath, EntityType entityType)
        {
            /*Entity segments can contain only letters, numbers, periods (.), hyphens (-), and underscores (-), paths can contain slashes (/) */
            var topicQueueNameRegex = new Regex(@"^[0-9A-Za-z_\.\-\/]+$");
            /*Except for subscriptions, these cannot contain slashes (/) */
            var subscriptionNameRegex = new Regex(@"^[0-9A-Za-z_\.\-]+$");
            var valid = true;

            switch (entityType)
            {
                case EntityType.Queue:
                    valid &= entityPath.Length <= QueuePathMaxLength;
                    valid &= topicQueueNameRegex.IsMatch(entityPath);
                    break;
                case EntityType.Topic:
                    valid &= entityPath.Length <= TopicPathMaxLength;
                    valid &= topicQueueNameRegex.IsMatch(entityPath);
                    break;
                case EntityType.Subscription:
                    valid &= entityPath.Length <= SubscriptionPathMaxLength;
                    valid &= subscriptionNameRegex.IsMatch(entityPath);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("entityType");
            }
            
            return valid;
        }
    }
}