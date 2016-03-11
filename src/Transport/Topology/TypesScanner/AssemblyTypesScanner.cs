namespace NServiceBus.AzureServiceBus.TypesScanner
{
    using System;

    class AssemblyTypesScanner : ITypesScanner, IEquatable<AssemblyTypesScanner>
    {
        private readonly string _assemblyName;

        public AssemblyTypesScanner(string assemblyName)
        {
            _assemblyName = assemblyName;
        }

        public override bool Equals(object obj)
        {
            var target = obj as AssemblyTypesScanner;
            return Equals(target);
        }

        public override int GetHashCode()
        {
            return _assemblyName.ToLower().GetHashCode();
        }

        public bool Equals(AssemblyTypesScanner other)
        {
            return other != null
                   && string.Equals(_assemblyName, other._assemblyName, StringComparison.OrdinalIgnoreCase);
        }
    }
}