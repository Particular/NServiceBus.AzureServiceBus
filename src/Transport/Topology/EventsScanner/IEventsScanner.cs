namespace NServiceBus.AzureServiceBus.EventsScanner
{
    using System;

    interface IEventsScanner
    {
         
    }

    class TypeEventsScanner : IEventsScanner
    {
        public Type Target { get; }

        public TypeEventsScanner(Type target)
        {
            Target = target;
        }
    }

    class AssemblyEventsScanner : IEventsScanner
    {
        public string AssemblyName { get; }

        public AssemblyEventsScanner(string assemblyName)
        {
            AssemblyName = assemblyName;
        }
    }
}