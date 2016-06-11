namespace NServiceBus.AzureServiceBus.Addressing
{
    public abstract class SanitizationStrategy
    {
        public abstract EntityType CanSanitize { get;  }
        public abstract string Sanitize(string entityPathOrName);
    }
}
