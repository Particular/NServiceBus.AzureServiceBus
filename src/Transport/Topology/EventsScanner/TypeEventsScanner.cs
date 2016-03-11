namespace NServiceBus.AzureServiceBus.EventsScanner
{
    using System;

    class TypeEventsScanner : IEventsScanner, IEquatable<TypeEventsScanner>
    {
        private readonly Type _target;

        public TypeEventsScanner(Type target)
        {
            _target = target;
        }

        public override bool Equals(object obj)
        {
            var target = obj as TypeEventsScanner;
            return Equals(target);
        }

        public override int GetHashCode()
        {
            return _target.GetHashCode();
        }

        public bool Equals(TypeEventsScanner other)
        {
            return other != null
                   && _target == other._target;
        }
    }
}