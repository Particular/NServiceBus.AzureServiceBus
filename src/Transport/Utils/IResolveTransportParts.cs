namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Generic;

    [Obsolete("Internal contract that shouldn't be exposed. Will be treated as an error from version 8.0.0. Will be removed in version 9.0.0.", false)]
    public interface IResolveTransportParts
    {
        object Resolve(Type typeToBuild);
        T Resolve<T>();
        IEnumerable<T> ResolveAll<T>();
        IEnumerable<object> ResolveAll(Type typeToBuild);
    }
}