namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using Microsoft.ServiceBus.Messaging;

    internal static class RetriableReceiveExceptionHandling
    {
        public static bool IsRetryable(MessagingException messagingException)
        {
            var inner = messagingException;

            while (inner != null)
            {
                if (inner.IsTransient ||
                    inner is QuotaExceededException ||
                    inner is DuplicateMessageException ||
                    inner is MessageLockLostException ||
                    inner is MessageNotFoundException ||
                    inner is MessagingEntityAlreadyExistsException ||
                    inner is MessagingEntityDisabledException ||
                    inner is SessionLockLostException)
                {
                    return true;
                }

                inner = inner.InnerException as MessagingException;
            }

            return false;
        }
    }
}