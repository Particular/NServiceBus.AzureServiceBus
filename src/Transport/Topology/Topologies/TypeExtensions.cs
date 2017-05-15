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
                var constructor = typeToResolve.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .OrderByDescending(c => c.GetParameters().Length)
                    .FirstOrDefault();

                if (constructor == null)
                    return (TReturnedType) Activator.CreateInstance(typeToResolve);

                return (TReturnedType) Activator.CreateInstance(typeToResolve, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, settings);
            }
            catch (Exception e)
            {
                throw new ArgumentException("Registered type must either have an empty constructor or a constructor that accepts ReadOnlySettings.", e);
            }
        }
    }
}