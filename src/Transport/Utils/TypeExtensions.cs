namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Collections.Generic;

    static class TypeExtensions
    {
        public static IEnumerable<Type> GetParentTypes(this Type target)
        {
            foreach (var i in target.GetInterfaces())
                yield return i;

            var currentBaseType = target.BaseType;
            var objectType = typeof(object);
            while (currentBaseType != null && currentBaseType != objectType)
            {
                yield return currentBaseType;
                currentBaseType = currentBaseType.BaseType;
            }
        }
    }
}