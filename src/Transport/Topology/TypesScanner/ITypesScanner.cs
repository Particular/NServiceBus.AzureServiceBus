namespace NServiceBus.AzureServiceBus.TypesScanner
{
    using System;
    using System.Collections.Generic;

    interface ITypesScanner
    {
        IEnumerable<Type> Scan();
    }
}