#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace NServiceBus
{
    static class ObsoleteMessages
    {
        public const string InternalizedContract = "Internal contract.";
        public const string ReplaceWithNewAPI = "Replaced with new API.";
        public const string DeprecatedAndNoLongerRequired = "Deprecated and no longer required.";
        public const string MaxDeliveryCountDeprecatedInFavorOfRecoverabilityAndImmediateRetries = "MaxDeliveryCount is automatically set to the maximum and not required. Overriding is not recommended, but possible using AzureServiceBusQueueSettings.DescriptionCustomizer().";
    }
}
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member