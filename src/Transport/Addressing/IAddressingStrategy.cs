namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;

    /// <summary>
    /// Looks like this may be a duplicate of upcoming IProvideDynamicRouting in the core, figure out where core ends and our logic begins
    /// </summary>

    public interface IAddressingStrategy
    {
        AzureServiceBusAddress GetAddressForPublishing(Type eventType);
        AzureServiceBusAddress GetAddressForSending(string destination);
    }

    public class AzureServiceBusAddress
    {
        public string EntityPath { get; set; }
        public string ConnectionString { get; set; }
    }
}
