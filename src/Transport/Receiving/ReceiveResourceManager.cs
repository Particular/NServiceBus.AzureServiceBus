namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using System;
    using System.Transactions;
    using Logging;
    using Microsoft.ServiceBus.Messaging;

    class ReceiveResourceManager : IEnlistmentNotification
    {
        BrokeredMessage receivedMessage;

        public ReceiveResourceManager(BrokeredMessage receivedMessage)
        {
            this.receivedMessage = receivedMessage;
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            preparingEnlistment.Prepared();
        }

        public void Commit(Enlistment enlistment)
        {
            try
            {
                receivedMessage.SafeComplete();
            }
            catch (Exception ex)
            {
                Log.Fatal(string.Format("A fatal exception occured while trying to complete a message, the exception was {0}", ex.Message), ex);
            }
           
            enlistment.Done();
        }

        public void Rollback(Enlistment enlistment)
        {
            try
            {
                // looks like abandon auto enlists in the current transaction, 
                // but as that one is rolling back now we can't do that.
                using (var scope = new TransactionScope(TransactionScopeOption.Suppress))
                {
                    receivedMessage.SafeAbandon();

                    scope.Complete();
                }
                
            }
            catch (Exception ex)
            {
                Log.Fatal(string.Format("A fatal exception occured while trying to abandon a message, the exception was {0}", ex.Message), ex);
            }

            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            enlistment.Done();
        }

        static ILog Log = LogManager.GetLogger(typeof(ReceiveResourceManager));
    }
}