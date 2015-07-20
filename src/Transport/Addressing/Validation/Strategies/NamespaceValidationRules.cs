namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;
    using System.Text.RegularExpressions;

    public class NamespaceValidationRules : IValidationStrategy
    {
        public bool IsValid(string entitypath, EntityType entityType)
        {
            /*Entity segments can contain only letters, numbers, periods (.), hyphens (-), and underscores (-), paths can contain slashes (/) */
            var topicQueueNameRegex = new Regex(@"^[0-9A-Za-z_\.\-\/]+$");
            /*Except for subscriptions, these cannot contain slashes (/) */
            var subscriptionNameRegex = new Regex(@"^[0-9A-Za-z_\.\-]+$");
            var valid = true;

            switch (entityType)
            {
                case EntityType.Queue:
                    valid &= entitypath.Length <= 260;
                    valid &= topicQueueNameRegex.IsMatch(entitypath);
                    break;
                case EntityType.Topic:
                    valid &= entitypath.Length <= 260;
                    valid &= topicQueueNameRegex.IsMatch(entitypath);
                    break;
                case EntityType.Subscription:
                    valid &= entitypath.Length <= 50;
                    valid &= subscriptionNameRegex.IsMatch(entitypath);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("entityType");
            }
            
            return valid;
        }
    }
}