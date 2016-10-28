namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;
    using Transport;

    [ObsoleteEx(Message = "Internal contract that shouldn't be exposed.", TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public interface IBatcher
    {
        IList<Batch> ToBatches(TransportOperations operations);
    }
}