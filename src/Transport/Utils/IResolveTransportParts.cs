namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Generic;

    [ObsoleteEx(Message = "Internal contract that shouldn't be exposed.", TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IResolveTransportParts
    {
        object Resolve(Type typeToBuild);
        T Resolve<T>();
        IEnumerable<T> ResolveAll<T>();
        IEnumerable<object> ResolveAll(Type typeToBuild);
    }
}