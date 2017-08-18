namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    class AssemblyTypesScanner : ITypesScanner, IEquatable<AssemblyTypesScanner>
    {
        public AssemblyTypesScanner(Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(nameof(assembly), "It's not possible to initialize an AssemblyTypesScanner without specifing an assembly");
            }

            this.assembly = assembly;
        }

        public bool Equals(AssemblyTypesScanner other)
        {
            return other != null
                   && assembly.Equals(other.assembly);
        }

        public IEnumerable<Type> Scan()
        {
            return assembly.GetTypes();
        }

        public override bool Equals(object obj)
        {
            var target = obj as AssemblyTypesScanner;
            return Equals(target);
        }

        public override int GetHashCode()
        {
            return assembly.GetHashCode();
        }

        Assembly assembly;
    }
}