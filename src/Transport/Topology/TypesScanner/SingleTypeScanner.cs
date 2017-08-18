namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Generic;

    class SingleTypeScanner : ITypesScanner, IEquatable<SingleTypeScanner>
    {
        public SingleTypeScanner(Type target)
        {
            this.target = target;
        }

        public bool Equals(SingleTypeScanner other)
        {
            return other != null
                   && target == other.target;
        }

        public IEnumerable<Type> Scan()
        {
            yield return target;
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

        Type target;
    }
}