namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using NServiceBus.ObjectBuilder.Common;

    public class FuncContainer : IContainer
    {
        IList<Tuple<Type, Func<object>>> funcs = new List<Tuple<Type, Func<object>>>();

        public void Dispose()
        {
        }

        public object Build(Type typeToBuild)
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
                var propertyInfos = result.GetType().GetProperties().Where(pi => pi.CanWrite).Where(pi => pi.PropertyType != result.GetType());
                var propsWithoutFuncs = propertyInfos
                    .Select(p => p.PropertyType)
                    .Intersect(funcs.Select(f => f.Item1)).ToList();

                propsWithoutFuncs.ForEach(propertyTypeToSet => propertyInfos.First(p => p.PropertyType == propertyTypeToSet)
                    .SetValue(result, Build(propertyTypeToSet), null));

                return result;

            }
            catch (Exception ex)
            {
                throw new Exception("Failed to build type: " + typeToBuild, ex);
            }
        }

        public IContainer BuildChildContainer()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<object> BuildAll(Type typeToBuild)
        {
            return funcs.Where(f => f.Item1 == typeToBuild)
                .Select(f => f.Item2())
                .ToList();
        }

        public void Configure(Type component, DependencyLifecycle dependencyLifecycle)
        {
            Register(component, DetermineFunc(component));
        }

        public void Configure<T>(Func<T> component, DependencyLifecycle dependencyLifecycle)
        {
            throw new NotImplementedException();
        }

        public void ConfigureProperty(Type component, string property, object value)
        {
        }

        public void RegisterSingleton(Type lookupType, object instance)
        {
            Register(lookupType, () => instance);
        }

        public bool HasComponent(Type componentType)
        {
            throw new NotImplementedException();
        }

        public void Release(object instance)
        {
        }

        public void Register(Type t, Func<object> func)
        {
            funcs.Add(new Tuple<Type, Func<object>>(t, func));
        }
        private Func<object> DetermineFunc(Type type)
        {
            var constructor = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                .OrderByDescending(c => c.GetParameters().Count())
                .FirstOrDefault();

            if (constructor == null)
                return () => Activator.CreateInstance(type);

            return () =>
            {
                var args = constructor.GetParameters().Select(p => Build(p.ParameterType)).ToArray();

                return Activator.CreateInstance(type, args, null);
            };

        }

    }
}