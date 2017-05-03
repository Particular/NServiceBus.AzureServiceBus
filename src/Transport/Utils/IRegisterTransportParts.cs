namespace NServiceBus.Transport.AzureServiceBus
{
    using System;

    interface IRegisterTransportPartsInternal
    {
        void Register<T>();
        void Register(Type t);
        void RegisterSingleton<T>();
        void Register<T>(Func<object> func);
    }
}