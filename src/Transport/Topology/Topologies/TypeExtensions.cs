namespace NServiceBus
{
    using System;
    using System.Linq;
    using System.Reflection;
    using Settings;

    static class TypeExtensions
    {
        // potentially we want to remove this when we simplify hooking customer owned implementations into the topology
        public static TReturnedType CreateInstance<TReturnedType>(this Type typeToResolve, ReadOnlySettings settings)
        {
            try
            {
                var defaultConstructor = typeToResolve.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .FirstOrDefault(c => c.GetParameters().Length == 0);

                if (defaultConstructor != null)
                {
                    return (TReturnedType)Activator.CreateInstance(typeToResolve, true);
                }

                return (TReturnedType)Activator.CreateInstance(typeToResolve, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new object[]
                {
                    settings
                }, null);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Registered type must either have an empty constructor or a constructor that accepts ReadOnlySettings.", e);
            }
        }
    }
}