namespace NServiceBus.Transport.AzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;

    /// <summary>
    /// Physical <see cref="BrokeredMessage"/> encoding.
    /// </summary>
    public enum SupportedBrokeredMessageBodyTypes
    {
        /// <summary>Serialized byte array.</summary>
        ByteArray,

        /// <summary>Stream of bytes.</summary>
        Stream
    }
}