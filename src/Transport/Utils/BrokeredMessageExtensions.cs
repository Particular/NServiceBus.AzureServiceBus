namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using Logging;
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Transactions;
    using Microsoft.ServiceBus.Messaging;

    static class BrokeredMessageExtensions
    {
        public static async Task<bool> SafeCompleteAsync(this BrokeredMessage msg)
        {
            try
            {
                await msg.CompleteAsync();
                return true;
            }
            catch (MessageLockLostException ex)
            {
                // It's too late to compensate the loss of a message lock. We should just ignore it so that it does not break the receive loop.
                Log.Warn($"A message lock lost exception occured while trying to complete a message, you may consider to increase the lock duration or reduce the batch size, the exception was {ex.Message}", ex);
            }
            catch (MessagingException ex)
            {
                // There is nothing we can do as the connection may have been lost, or the underlying queue may have been removed.
                // If Complete() fails with this exception, the only recourse is to receive another message.
                Log.Warn($"A messaging exception occured while trying to complete a message, this might imply that the connection was lost or the underlying queue got removed, the exception was {ex.Message}", ex);
            }
            catch (ObjectDisposedException ex)
            {
                // There is nothing we can do as the object has already been disposed elsewhere
                Log.Warn($"An object disposed exception occured while trying to complete a message, this might imply that the connection was lost or the underlying queue got removed, the exception was {ex.Message}", ex);
            }
            catch (TransactionException ex)
            {
                // ASB Sdk beat us to it
                Log.Warn($"A transaction exception occured while trying to complete a message, this probably means that the Azure ServiceBus SDK has rolled back the transaction already, the exception was {ex.Message}", ex);
            }
            catch (TimeoutException ex)
            {
                // took to long
                Log.Warn($"A timeout exception occured while trying to complete a message, the exception was {ex.Message}", ex);
            }
            return false;
        }

        public static async Task<bool> SafeAbandonAsync(this BrokeredMessage msg)
        {
            try
            {
                await msg.AbandonAsync();
                return true;
            }
            catch (MessageLockLostException ex)
            {
                // It's too late to compensate the loss of a message lock. We should just ignore it so that it does not break the receive loop.
                Log.Warn($"A message lock lost exception occured while trying to abandon a message, you may consider to increase the lock duration or reduce the batch size, the exception was {ex.Message}", ex);
            }
            catch (MessagingException ex)
            {
                // There is nothing we can do as the connection may have been lost, or the underlying queue may have been removed.
                // If Abandon() fails with this exception, the only recourse is to receive another message.
                Log.Warn($"A messaging exception occured while trying to abandon a message, this might imply that the connection was lost or the underlying queue got removed, the exception was {ex.Message}", ex);
            }
            catch (ObjectDisposedException ex)
            {
                // There is nothing we can do as the object has already been disposed elsewhere
                Log.Warn($"An object disposed exception occured while trying to abandon a message, this might imply that the connection was lost or the underlying queue got removed, the exception was {ex.Message}", ex);
            }
            catch (TransactionException ex)
            {
                // ASB Sdk beat us to it
                Log.Warn($"A transaction exception occured while trying to abandon a message, this probably means that the Azure ServiceBus SDK has rolled back the transaction already, the exception was {ex.Message}", ex);
            }
            catch (TimeoutException ex)
            {
                // took to long
                Log.Warn($"A timeout exception occured while trying to abandon a message, the exception was {ex.Message}", ex);
            }
            return false;
        }

        public static long TotalEstimatedSize(this BrokeredMessage msg)
        {
            var standardPropertiesSize = GetSize(msg.ContentType) +
                                         GetSize(msg.CorrelationId) +
                                         4 + //GetSize(msg.DeliveryCount) + // can't use until message is sent/received
                                         8 + //GetSize(msg.EnqueuedSequenceNumber) +
                                         8 + //GetSize(msg.EnqueuedTimeUtc) +
                                         8 + //GetSize(msg.ExpiresAtUtc) +
                                         GetSize(msg.ForcePersistence) +
                                         GetSize(msg.IsBodyConsumed) +
                                         GetSize(msg.Label) +
                                         8 + //GetSize(msg.LockedUntilUtc) +
                                         16 + //GetSize(msg.LockToken) +
                                         GetSize(msg.MessageId) +
                                         GetSize(msg.PartitionKey) +
                                         GetSize(msg.ReplyTo) +
                                         GetSize(msg.ReplyToSessionId) +
                                         GetSize(msg.ScheduledEnqueueTimeUtc) +
                                         8 + //GetSize(msg.SequenceNumber) +
                                         GetSize(msg.SessionId) +
                                         GetSize(msg.SessionId) +
                                         GetSize((int) msg.State) +
                                         GetSize(msg.TimeToLive) +
                                         GetSize(msg.To) +
                                         GetSize(msg.ViaPartitionKey);

            var customPropertiesSize = msg.Properties.Sum(property => GetSize(property.Key) + GetSize(property.Value as string));

            var bodySize = msg.Size; // body?

            var totalWithoutAdditionalPercentage = standardPropertiesSize + customPropertiesSize + bodySize;

            return (long) (totalWithoutAdditionalPercentage * 1.1);
        }

        static int GetSize(string value) => value != null ? Encoding.UTF8.GetByteCount(value) : 0;

        static int GetSize(TimeSpan value) => 8;

        static int GetSize(DateTime value) => 8;

        static int GetSize(long value) => 8;

        static int GetSize(Guid value) => 16;

        static int GetSize(int value) => 4;

        static int GetSize(short value) => 2;

        static int GetSize(bool value) => 1;

        static ILog Log = LogManager.GetLogger(typeof(BrokeredMessageExtensions));
    }
}