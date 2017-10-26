namespace NServiceBus.AzureServiceBus.Utils
{
    using System;
    using Microsoft.ServiceBus.Messaging;

    static class ExceptionExtensions
    {
        /// <summary>Exception is transient if it's derived from <see cref="MessagingException"/> or is of type <see cref="TimeoutException"/></summary>
        public static bool IsTransientException(this Exception exception)
        {
            var messagingException = exception as MessagingException;
            return messagingException?.IsTransient ?? (exception is TimeoutException || exception is OperationCanceledException);
        }
    }
}