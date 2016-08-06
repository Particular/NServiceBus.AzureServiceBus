namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Generic;

    interface ITypesScanner
    {
        IEnumerable<Type> Scan();
    }
}