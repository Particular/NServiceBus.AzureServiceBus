﻿namespace NServiceBus.AzureServiceBus
{
    public interface IClientSideSubscriptionFilter
    {
        /// <summary>
        /// executes a filter in memory, if it is impossible to inject it into the broker (eventhub case)
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        bool Execute(object message);
    }
}