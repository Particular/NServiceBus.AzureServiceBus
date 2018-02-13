namespace NServiceBus.Transport.AzureServiceBus
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Transactions;
    using Logging;
    using Microsoft.ServiceBus.Messaging;

    static class BrokeredMessageExtensions
    {
        public static async Task<bool> SafeCompleteAsync(this BrokeredMessage msg)
        {
            try
            {
                await msg.CompleteAsync().ConfigureAwait(false);
                return true;
            }
            catch (MessageLockLostException ex)
            {
                // It's too late to compensate the loss of a message lock. We should just ignore it so that it does not break the receive loop.
                Log.Warn($"A message lock lost exception occurred while trying to complete a message, you may consider to increase the lock duration or reduce the prefetch count, the exception was {ex.Message}", ex);
            }
            catch (MessagingException ex)
            {
                // There is nothing we can do as the connection may have been lost, or the underlying queue may have been removed.
                // If Complete() fails with this exception, the only recourse is to receive another message.
                Log.Warn($"A messaging exception occurred while trying to complete a message, this might imply that the connection was lost or the underlying queue got removed, the exception was {ex.Message}", ex);
            }
            catch (ObjectDisposedException ex)
            {
                // There is nothing we can do as the object has already been disposed elsewhere
                Log.Warn($"An object disposed exception occurred while trying to complete a message, this might imply that the connection was lost or the underlying queue got removed, the exception was {ex.Message}", ex);
            }
            catch (TransactionException ex)
            {
                // ASB Sdk beat us to it
                Log.Warn($"A transaction exception occurred while trying to complete a message, this probably means that the Azure ServiceBus SDK has rolled back the transaction already, the exception was {ex.Message}", ex);
            }
            catch (TimeoutException ex)
            {
                // took to long
                Log.Warn($"A timeout exception occurred while trying to complete a message, the exception was {ex.Message}", ex);
            }
            return false;
        }

        public static async Task<bool> SafeAbandonAsync(this BrokeredMessage msg, IDictionary<string, object> propertiesToModify = null)
        {
            try
            {
                if (propertiesToModify == null)
                {
                    await msg.AbandonAsync().ConfigureAwait(false);
                }
                else
                {
                    await msg.AbandonAsync(propertiesToModify).ConfigureAwait(false);
                }

                return true;
            }
            catch (MessageLockLostException ex)
            {
                // It's too late to compensate the loss of a message lock. We should just ignore it so that it does not break the receive loop.
                Log.Warn($"A message lock lost exception occurred while trying to abandon a message, you may consider to increase the lock duration or reduce the prefetch count, the exception was {ex.Message}", ex);
            }
            catch (MessagingException ex)
            {
                // There is nothing we can do as the connection may have been lost, or the underlying queue may have been removed.
                // If Abandon() fails with this exception, the only recourse is to receive another message.
                Log.Warn($"A messaging exception occurred while trying to abandon a message, this might imply that the connection was lost or the underlying queue got removed, the exception was {ex.Message}", ex);
            }
            catch (ObjectDisposedException ex)
            {
                // There is nothing we can do as the object has already been disposed elsewhere
                Log.Warn($"An object disposed exception occurred while trying to abandon a message, this might imply that the connection was lost or the underlying queue got removed, the exception was {ex.Message}", ex);
            }
            catch (TransactionException ex)
            {
                // ASB Sdk beat us to it
                Log.Warn($"A transaction exception occurred while trying to abandon a message, this probably means that the Azure ServiceBus SDK has rolled back the transaction already, the exception was {ex.Message}", ex);
            }
            catch (TimeoutException ex)
            {
                // took to long
                Log.Warn($"A timeout exception occurred while trying to abandon a message, the exception was {ex.Message}", ex);
            }
            return false;
        }

        public static long EstimatedSize(this BrokeredMessage message)
        {
            message.Properties.TryGetValue(BrokeredMessageHeaders.EstimatedMessageSize, out var size);
            return Convert.ToInt64(size);
        }

        static ILog Log = LogManager.GetLogger(typeof(BrokeredMessageExtensions));
    }
}