namespace NServiceBus.Azure.Transports.WindowsAzureServiceBus
{
    using System;
    using System.Transactions;
    using Logging;

    class SendResourceManager : IEnlistmentNotification
    {
        Action onCommit;

        public SendResourceManager(Action onCommit )
        {
            this.onCommit = onCommit;
        }

        public void Prepare(PreparingEnlistment preparingEnlistment)
        {
            preparingEnlistment.Prepared();
        }

        public void Commit(Enlistment enlistment)
        {
            try
            {
                onCommit();
            }
            catch (Exception ex)
            {
                Log.Fatal(string.Format("A fatal exception occured while trying to send a message, the exception was {0}", ex.Message), ex);
            }
            enlistment.Done();
        }

        public void Rollback(Enlistment enlistment)
        {
            enlistment.Done();
        }

        public void InDoubt(Enlistment enlistment)
        {
            enlistment.Done();
        }

        static ILog Log = LogManager.GetLogger(typeof(SendResourceManager));
    }
}