namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Transactions;
    using Logging;
    using Microsoft.ServiceBus.Messaging;

    static class MessageReceiverExtensions
    {
        public static async Task<bool> SafeCompleteBatchAsync(this IMessageReceiverInternal messageReceiver, IEnumerable<Guid> lockTokens)
        {
            try
            {
                await messageReceiver.CompleteBatchAsync(lockTokens).ConfigureAwait(false);
                return true;
            }
            catch (MessageLockLostException ex)
            {
                // It's too late to compensate the loss of a message lock. We should just ignore it so that it does not break the receive loop.
                Log.Warn($"A message lock lost exception occurred while trying to complete a batch of messages, you may consider to increase the lock duration or reduce the prefetch count, the exception was {ex.Message}", ex);
            }
            catch (MessagingException ex)
            {
                // There is nothing we can do as the connection may have been lost, or the underlying queue may have been removed.
                // If Complete() fails with this exception, the only recourse is to receive another message.
                Log.Warn($"A messaging exception occurred while trying to complete a batch of message, this might imply that the connection was lost or the underlying queue got removed, the exception was {ex.Message}", ex);
            }
            catch (ObjectDisposedException ex)
            {
                // There is nothing we can do as the object has already been disposed elsewhere
                Log.Warn($"An object disposed exception occurred while trying to complete a batch of messages, this might imply that the connection was lost or the underlying queue got removed, the exception was {ex.Message}", ex);
            }
            catch (TransactionException ex)
            {
                // ASB Sdk beat us to it
                Log.Warn($"A transaction exception occurred while trying to complete a batch of messages, this probably means that the Azure ServiceBus SDK has rolled back the transaction already, the exception was {ex.Message}", ex);
            }
            catch (TimeoutException ex)
            {
                // took to long
                Log.Warn($"A timeout exception occurred while trying to complete a batch of messages, the exception was {ex.Message}", ex);
            }
            return false;
        }

        static ILog Log = LogManager.GetLogger(typeof(MessageReceiverExtensions));
    }
}