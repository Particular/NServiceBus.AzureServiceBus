namespace NServiceBus.Transport.AzureServiceBus
{
    using System;

    interface IResolveTransportPartsInternal
    {
        object Resolve(Type typeToBuild);
        T Resolve<T>();
    }
}