namespace NServiceBus.AzureServiceBus
{
    using Features;

    class TransactionScopeSuppressFeature : Feature
    {
        protected override void Setup(FeatureConfigurationContext context)
        {
            context.Container.ConfigureComponent(b => new TransactionScopeSuppressBehavior(), DependencyLifecycle.InstancePerCall);

            context.Pipeline.Register(new TransactionScopeSuppressBehavior.Registration());
        }
    }
}