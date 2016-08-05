namespace NServiceBus.AzureServiceBus
{
    using System;
    using System.Threading.Tasks;
    using System.Transactions;
    using Features;
    using Microsoft.ServiceBus.Messaging;
    using Pipeline;

    public class BrokeredMessageReceiveContext : ReceiveContext
    {
        public BrokeredMessageReceiveContext(BrokeredMessage message, EntityInfo entity, ReceiveMode receiveMode)
        {
            IncomingBrokeredMessage = message;
            Entity = entity;
            ReceiveMode = receiveMode;
        }

        public BrokeredMessage IncomingBrokeredMessage { get; }

        public EntityInfo Entity { get; }

        // Dispatcher needs to compare this with requested consistency guarantees, cannot do default (postponed) dispatch if there is no completion step (ReceiveAndDelete)
        public ReceiveMode ReceiveMode { get; }

        // while recovering, send via must be avoided as it will be rolled back
        public bool Recovering { get; set; }

    }

    class TransactionScopeSuppressBehavior : Behavior<IIncomingPhysicalMessageContext>
    {
        public override async Task Invoke(IIncomingPhysicalMessageContext context, Func<Task> next)
        {
            if (Transaction.Current != null)
            {
                using (var tx = new TransactionScope(TransactionScopeOption.Suppress, TransactionScopeAsyncFlowOption.Enabled))
                {
                    await next().ConfigureAwait(false);

                    tx.Complete();
                }
            }
            else
            {
                await next().ConfigureAwait(false);
            }
        }

        public class Registration : RegisterStep
        {
            public Registration() : base("HandlerTransactionScopeSuppressWrapper", typeof(TransactionScopeSuppressBehavior), "Makes sure that the handlers gets wrapped in a suppress transaction scope, preventing the ASB transaction scope from promoting")
            {
                InsertBefore("ExecuteUnitOfWork");
            }
        }
    }

    class TransactionScopeSuppress : Feature
    {
        public TransactionScopeSuppress()
        {
            EnableByDefault();
        }

        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Container.ConfigureComponent(b => new TransactionScopeSuppressBehavior(), DependencyLifecycle.InstancePerCall);

            context.Pipeline.Register(new TransactionScopeSuppressBehavior.Registration());
        }
    }
}