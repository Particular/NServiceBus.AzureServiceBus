namespace NServiceBus.AzureServiceBus.EventsScanner
{
    using System;

    class AssemblyEventsScanner : IEventsScanner, IEquatable<AssemblyEventsScanner>
    {
        private readonly string _assemblyName;

        public AssemblyEventsScanner(string assemblyName)
        {
            _assemblyName = assemblyName;
        }

        public override bool Equals(object obj)
        {
            var target = obj as AssemblyEventsScanner;
            return Equals(target);
        }

        public override int GetHashCode()
        {
            return _assemblyName.ToLower().GetHashCode();
        }

        public bool Equals(AssemblyEventsScanner other)
        {
            return other != null
                   && string.Equals(_assemblyName, other._assemblyName, StringComparison.OrdinalIgnoreCase);
        }
    }
}