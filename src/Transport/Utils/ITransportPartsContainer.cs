namespace NServiceBus.Transport.AzureServiceBus
{
    using System;

    [Obsolete("Internal contract that shouldn't be exposed. Will be treated as an error from version 8.0.0. Will be removed in version 9.0.0.", false)]
    public interface ITransportPartsContainer : IRegisterTransportParts, IResolveTransportParts { }
}