namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using NServiceBus.ObjectBuilder;
    using NServiceBus.ObjectBuilder.Common;

    public class FuncBuilder : IBuilder, IConfigureComponents
    {
        /// <summary>
        /// The container that will be used to create objects and configure components.
        /// </summary>
        private IContainer container;

        public FuncBuilder(IContainer container)
        {
            this.container = container;
            container.RegisterSingleton(typeof(IBuilder), this);
        }

        public IComponentConfig ConfigureComponent(Type concreteComponent, DependencyLifecycle instanceLifecycle)
        {
            container.Configure(concreteComponent, instanceLifecycle);

            return new ComponentConfig(concreteComponent, container);
        }

        public IComponentConfig<T> ConfigureComponent<T>(DependencyLifecycle instanceLifecycle)
        {
            container.Configure(typeof(T), instanceLifecycle);

            return new ComponentConfig<T>(container);
        }

        public IComponentConfig<T> ConfigureComponent<T>(Func<T> componentFactory, DependencyLifecycle instanceLifecycle)
        {
            container.Configure(componentFactory, instanceLifecycle);

            return new ComponentConfig<T>(container);
        }

        public IComponentConfig<T> ConfigureComponent<T>(Func<IBuilder, T> componentFactory, DependencyLifecycle instanceLifecycle)
        {
            container.Configure(() => componentFactory(this), instanceLifecycle);

            return new ComponentConfig<T>(container);
        }

        public IConfigureComponents ConfigureProperty<T>(Expression<Func<T, object>> property, object value)
        {
            var prop = Reflect<T>.GetProperty(property);

            return ((IConfigureComponents)this).ConfigureProperty<T>(prop.Name, value);
        }

        public IConfigureComponents ConfigureProperty<T>(string propertyName, object value)
        {
            container.ConfigureProperty(typeof(T), propertyName, value);

            return this;
        }

        IConfigureComponents IConfigureComponents.RegisterSingleton(Type lookupType, object instance)
        {
            container.RegisterSingleton(lookupType, instance);
            return this;
        }

        public IConfigureComponents RegisterSingleton<T>(T instance)
        {
            container.RegisterSingleton(typeof(T), instance);
            return this;
        }

        public bool HasComponent<T>()
        {
            return container.HasComponent(typeof(T));
        }

        public bool HasComponent(Type componentType)
        {
            return container.HasComponent(componentType);
        }


        public IBuilder CreateChildBuilder()
        {
            return new FuncBuilder
            (
                container.BuildChildContainer()
            );
        }

        public void Dispose()
        {
            //Injected at compile time
        }

        void DisposeManaged()
        {
            container?.Dispose();
        }

        public T Build<T>()
        {
            return (T)container.Build(typeof(T));
        }

        public object Build(Type typeToBuild)
        {
            return container.Build(typeToBuild);
        }

        IEnumerable<object> IBuilder.BuildAll(Type typeToBuild)
        {
            return container.BuildAll(typeToBuild);
        }

        void IBuilder.Release(object instance)
        {
            container.Release(instance);
        }

        public IEnumerable<T> BuildAll<T>()
        {
            return container.BuildAll(typeof(T)).Cast<T>();
        }

        public void BuildAndDispatch(Type typeToBuild, Action<object> action)
        {
            var o = container.Build(typeToBuild);
            action(o);
        }
    }

    class ComponentConfig : IComponentConfig
    {
        Type component;
        IContainer container;

        public ComponentConfig(Type component, IContainer container)
        {
            this.component = component;
            this.container = container;
        }

        IComponentConfig IComponentConfig.ConfigureProperty(string name, object value)
        {
            container.ConfigureProperty(component, name, value);

            return this;
        }
    }

    class ComponentConfig<T> : ComponentConfig, IComponentConfig<T>
    {
        public ComponentConfig(IContainer container) : base(typeof(T), container)
        {
        }

        IComponentConfig<T> IComponentConfig<T>.ConfigureProperty(Expression<Func<T, object>> property, object value)
        {
            var prop = Reflect<T>.GetProperty(property);

            ((IComponentConfig)this).ConfigureProperty(prop.Name, value);

            return this;
        }
    }

    static class Reflect<TTarget>
    {
        public static PropertyInfo GetProperty(Expression<Func<TTarget, object>> property)
        {
            var info = GetMemberInfo(property, false) as PropertyInfo;
            if (info == null) throw new ArgumentException("Member is not a property");

            return info;
        }

        internal static List<TTarget> GetEnumValues()
        {
            return Enum.GetValues(typeof(TTarget))
                .Cast<TTarget>()
                .ToList();
        }

        public static PropertyInfo GetProperty(Expression<Func<TTarget, object>> property, bool checkForSingleDot)
        {
            return GetMemberInfo(property, checkForSingleDot) as PropertyInfo;
        }

        static MemberInfo GetMemberInfo(Expression member, bool checkForSingleDot)
        {
            if (member == null) throw new ArgumentNullException("member");

            var lambda = member as LambdaExpression;
            if (lambda == null) throw new ArgumentException("Not a lambda expression", "member");

            MemberExpression memberExpr = null;

            // The Func<TTarget, object> we use returns an object, so first statement can be either 
            // a cast (if the field/property does not return an object) or the direct member access.
            if (lambda.Body.NodeType == ExpressionType.Convert)
            {
                // The cast is an unary expression, where the operand is the 
                // actual member access expression.
                memberExpr = ((UnaryExpression)lambda.Body).Operand as MemberExpression;
            }
            else if (lambda.Body.NodeType == ExpressionType.MemberAccess)
            {
                memberExpr = lambda.Body as MemberExpression;
            }

            if (memberExpr == null) throw new ArgumentException("Not a member access", "member");

            if (checkForSingleDot)
            {
                if (memberExpr.Expression is ParameterExpression)
                {
                    return memberExpr.Member;
                }
                throw new ArgumentException("Argument passed contains more than a single dot which is not allowed: " + member, "member");
            }

            return memberExpr.Member;
        }

    }
}