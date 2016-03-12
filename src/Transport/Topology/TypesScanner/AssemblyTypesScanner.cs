namespace NServiceBus.AzureServiceBus.TypesScanner
{
    using System;
    using System.Reflection;

    class AssemblyTypesScanner : ITypesScanner, IEquatable<AssemblyTypesScanner>
    {
        private readonly Assembly _assembly;

        public AssemblyTypesScanner(Assembly assembly)
        {
             if (assembly == null)
                throw new ArgumentNullException(nameof(assembly), "It's not possible to initialize an AssemblyTypesScanner without specifing an assembly");

            _assembly = assembly;
        }

        public override bool Equals(object obj)
        {
            var target = obj as AssemblyTypesScanner;
            return Equals(target);
        }

        public override int GetHashCode()
        {
            return _assembly.GetHashCode();
        }

        public bool Equals(AssemblyTypesScanner other)
        {
            return other != null
                   && _assembly.Equals(other._assembly);
        }
    }
}