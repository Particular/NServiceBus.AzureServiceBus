//using System.Threading.Tasks;
//using NServiceBus;
//using NServiceBus.AzureServiceBus.AcceptanceTests.Infrastructure;
//
//public class ConfigureAzureServiceBusTransport
//{
//    public Task Configure(BusConfiguration config)
//    {
//        // TODO: If https://github.com/Particular/NServiceBus/pull/3203 is not merged, we need a settings in order to get the connection string
//        //config.UseTransport<AzureServiceBusTransport>()
//        //    .ConnectionString(settings["Transport.ConnectionString"]);
//
//        config.RegisterComponents(c =>
//        {
//            c.ConfigureComponent<TestIndependenceData>(DependencyLifecycle.SingleInstance);
//            c.ConfigureComponent<TestIndependenceMutator>(DependencyLifecycle.SingleInstance);
//        });
//
//        config.Pipeline.Register("TestIndependenceBehavior", typeof(TestIndependenceSkipBehavior), "Skips messages not created during the current test.");
//
//        return Task.FromResult(0);
//    }
//
//    public Task Cleanup()
//    {
//        return Task.FromResult(0);
//    }
//}
