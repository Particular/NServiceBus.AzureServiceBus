namespace NServiceBus.AzureServiceBus.Utils
{
    using System;
    using Microsoft.ServiceBus.Messaging;
    using Transport.AzureServiceBus;

    static class ExceptionExtensions
    {
        /// <summary>
        /// Exception is transient if it's derived from <see cref="MessagingException"/> and has its property <see cref="MessagingException.IsTransient"/> set to true or 
        /// action is one of the <see cref="ExceptionReceivedAction"/> actions.
        /// </summary>
        public static bool IsTransientException(this Exception exception, string action = "")
        {
            var messagingException = exception as MessagingException;
            return messagingException?.IsTransient ?? 
                action.Equals(ExceptionReceivedAction.RenewLock) 
                || action.Equals(ExceptionReceivedAction.Receive) 
                || action.Equals(ExceptionReceivedAction.ReceiveMessage);
        }
    }
}