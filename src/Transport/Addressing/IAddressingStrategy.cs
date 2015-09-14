namespace NServiceBus.AzureServiceBus.Addressing
{
    using System;

    /// <summary>
    /// Looks like this may be a duplicate of upcoming IProvideDynamicRouting in the core, figure out where core ends and our logic begins
    /// </summary>

    public interface IAddressingStrategy
    {
        EntityInfo[] GetEntitiesForPublishing(Type eventType);
        EntityInfo[] GetEntitiesForSending(string destination);
    }

}
