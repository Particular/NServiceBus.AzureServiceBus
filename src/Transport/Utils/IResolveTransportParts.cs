namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Generic;

    public interface IResolveTransportParts
    {
        object Resolve(Type typeToBuild);
        T Resolve<T>();
        IEnumerable<T> ResolveAll<T>();
        IEnumerable<object> ResolveAll(Type typeToBuild);
    }
}