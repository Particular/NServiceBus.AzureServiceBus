﻿namespace NServiceBus.Transport.AzureServiceBus
{
    using System.Collections.Generic;

    [ObsoleteEx(RemoveInVersion = "9.0", TreatAsErrorFromVersion = "8.0", ReplacementTypeOrMember = "IPartitioningStrategy")]
    public interface INamespacePartitioningStrategy
    {
        IEnumerable<RuntimeNamespaceInfo> GetNamespaces(PartitioningIntent partitioningIntent);
    }
}
