namespace NServiceBus.AzureServiceBus
{
    using System;

    public interface IRegisterTransportParts
    {
        void Register<T>();
        void Register(Type t);
        void RegisterSingleton<T>();
        void RegisterSingleton(Type t);
        void Register<T>(Func<object> func);
        void Register(Type t, Func<object> func);
    }
}