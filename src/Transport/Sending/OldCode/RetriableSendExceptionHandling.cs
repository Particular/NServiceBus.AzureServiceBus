namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;

    internal static class RetriableSendExceptionHandling
    {
        public static bool IsRetryable(MessagingException messagingException)
        {
            var inner = messagingException;

            while (inner != null)
            {
                if (inner.IsTransient ||
                    inner is MessagingEntityDisabledException)
                {
                    return true;
                }

                inner = inner.InnerException as MessagingException;
            }

            return false;
        }
    }
}