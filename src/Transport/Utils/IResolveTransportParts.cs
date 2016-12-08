namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Generic;

    interface IResolveTransportPartsInternal
    {
        object Resolve(Type typeToBuild);
        T Resolve<T>();
        IEnumerable<T> ResolveAll<T>();
        IEnumerable<object> ResolveAll(Type typeToBuild);
    }
}