namespace NServiceBus.AzureServiceBus
{
    using NServiceBus.Settings;

    //using System;
    //using NServiceBus.Transports;

    public interface ITopology {
        //Func<ICreateQueues> GetQueueCreatorFactory();
        //Func<CriticalError, IPushMessages> GetMessagePumpFactory();
        //Func<IDispatchMessages> GetDispatcherFactory();
        void ApplyDefaults(SettingsHolder settings);
        void InitializeContainer(SettingsHolder settings);
    }
}