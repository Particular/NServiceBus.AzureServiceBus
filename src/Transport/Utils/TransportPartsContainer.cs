namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    class TransportPartsContainer : ITransportPartsContainerInternal
    {
        List<Tuple<Type, Func<object>>> funcs = new List<Tuple<Type, Func<object>>>();

        public TransportPartsContainer()
        {
            Register<IRegisterTransportPartsInternal>(() => this);
            Register<IResolveTransportPartsInternal>(() => this);
        }

        public void Register<T>()
        {
            Register(typeof(T));
        }

        public void Register(Type t)
        {
            Register(t, DetermineFunc(t));
        }

        public void RegisterSingleton<T>()
        {
            RegisterSingleton(typeof(T));
        }

        public void RegisterSingleton(Type t)
        {
            var i = DetermineFunc(t)();
            Register(t, () => i);
        }

        public void Register<T>(Func<object> func)
        {
            Register(typeof(T), func);
        }

        public void Register(Type t, Func<object> func)
        {
            funcs.Add(new Tuple<Type, Func<object>>(t, func));
        }

        public object Resolve(Type typeToBuild)
        {
            try
            {
                var fn = funcs.FirstOrDefault(f => typeToBuild.IsAssignableFrom(f.Item1));

                if (fn == null)
                {
                    var @interface = typeToBuild.GetInterfaces().FirstOrDefault();
                    if (@interface != null)
                    {
                        fn = funcs.FirstOrDefault(f => @interface.IsAssignableFrom(f.Item1));
                    }
                }

                object result;

                if (fn != null)
                {
                    result = fn.Item2();
                }
                else
                {
                    result = Activator.CreateInstance(typeToBuild);
                }

                //enable property injection
                var propertyInfos = result.GetType().GetProperties()
                    .Where(pi => pi.CanWrite)
                    .Where(pi => pi.PropertyType != result.GetType())
                    .ToList();
                var propsWithoutFuncs = propertyInfos
                    .Select(p => p.PropertyType)
                    .Intersect(funcs.Select(f => f.Item1)).ToList();

                propsWithoutFuncs.ForEach(propertyTypeToSet => propertyInfos.First(p => p.PropertyType == propertyTypeToSet)
                    .SetValue(result, Resolve(propertyTypeToSet), null));

                return result;

            }
            catch (Exception ex)
            {
                throw new Exception("Failed to build type: " + typeToBuild,ex);
            }
        }

        public T Resolve<T>()
        {
            try
            {
                return (T) Resolve(typeof(T));
            }
            catch (Exception exception)
            {
                throw new ApplicationException($"Could not build {typeof(T)}", exception);
            }
        }

        public IEnumerable<T> ResolveAll<T>()
        {
            return funcs.Where(f => f.Item1 == typeof(T))
                .Select(f => (T)f.Item2())
                .ToList();
        }

        public IEnumerable<object> ResolveAll(Type typeToBuild)
        {
            return funcs.Where(f => f.Item1 == typeToBuild)
                .Select(f => f.Item2())
                .ToList();
        }

        Func<object> DetermineFunc(Type type)
        {
            var constructor = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .OrderByDescending(c => c.GetParameters().Count())
                .FirstOrDefault();

            if (constructor == null)
                return () => Activator.CreateInstance(type);

            return () =>
            {
                var args = constructor.GetParameters().Select(p => Resolve(p.ParameterType)).ToArray();

                return Activator.CreateInstance(type, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, args, null);
            };

        }
    }
}