namespace NServiceBus.AzureServiceBus.TypesScanner
{
    using System;
    using System.Collections.Generic;

    class SingleTypeScanner : ITypesScanner, IEquatable<SingleTypeScanner>
    {
        private readonly Type _target;

        public SingleTypeScanner(Type target)
        {
            _target = target;
        }

        public override bool Equals(object obj)
        {
            var target = obj as SingleTypeScanner;
            return Equals(target);
        }

        public override int GetHashCode()
        {
            return _target.GetHashCode();
        }

        public IEnumerable<Type> Scan()
        {
            yield return _target;
        }

        public bool Equals(SingleTypeScanner other)
        {
            return other != null
                   && _target == other._target;
        }
    }
}