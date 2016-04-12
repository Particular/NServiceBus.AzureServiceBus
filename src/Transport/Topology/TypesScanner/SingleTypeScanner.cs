namespace NServiceBus.AzureServiceBus.TypesScanner
{
    using System;
    using System.Collections.Generic;

    class SingleTypeScanner : ITypesScanner, IEquatable<SingleTypeScanner>
    {
        private readonly Type target;

        public SingleTypeScanner(Type target)
        {
            this.target = target;
        }

        public override bool Equals(object obj)
        {
            var target = obj as SingleTypeScanner;
            return Equals(target);
        }

        public override int GetHashCode()
        {
            return target.GetHashCode();
        }

        public IEnumerable<Type> Scan()
        {
            yield return target;
        }

        public bool Equals(SingleTypeScanner other)
        {
            return other != null
                   && target == other.target;
        }
    }
}