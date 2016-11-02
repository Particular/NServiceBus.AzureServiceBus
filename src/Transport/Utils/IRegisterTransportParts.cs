namespace NServiceBus.Transport.AzureServiceBus
{
    using System;

    [ObsoleteEx(Message = ObsoleteMessages.WillBeInternalized, TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
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