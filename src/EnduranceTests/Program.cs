namespace NServiceBus.AzureServiceBus.EnduranceTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using NServiceBus.AzureServiceBus.EnduranceTests.Commands;
    using NServiceBus.AzureServiceBus.EnduranceTests.TestUtils;

    internal class Program
    {
        private static void Main(string[] args)
        {
            LoggingHelper.Configure();

            var cts = new CancellationTokenSource();

            const string endpointName = "EnduranceTest";

            Console.WriteLine("CTRL + C to stop test. Press Enter to continue.");
            Console.ReadLine();

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            for (;;)
            {
                try
                {
                    RunEndpointAsync(endpointName, cts.Token).Wait(cts.Token);

                    break;
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Test Cancelled. Press ENTER to continue.");
                    Console.ReadLine();

                    break;
                }
                catch (Exception ex)
                {
                    LoggingHelper.Log.Error(ex, "Caught RunEndpointAsync Exception");
                }
            }
        }

        private static async Task RunEndpointAsync(string endpointName, CancellationToken ct)
        {
            var connectionString = TestEnvironment.AzureServiceBus;

            var endpointConfiguration = new EndpointConfiguration();
            endpointConfiguration.EndpointName(endpointName);
            endpointConfiguration.UseTransport<AzureServiceBusTransport>()
                .ConnectionString(connectionString);
            endpointConfiguration.UseSerialization<JsonSerializer>();
            endpointConfiguration.UsePersistence<InMemoryPersistence>();
            endpointConfiguration.SendFailedMessagesTo("error");

            var endpoint = await Endpoint.Start(endpointConfiguration);

            Console.WriteLine(endpointName + " started");

            await endpoint.Send<Heartbeat>(cmd =>
            {
            }, TestSettings.SendOptions);
            try
            {
                do
                {
                    await Task.Delay(60000, ct);
                    Console.WriteLine("NServiceBus Endpoint Running. Press CTRL+C to Cancel");
                } while (!ct.IsCancellationRequested);
            }
            finally
            {
                await endpoint.Stop();
            }
        }
    }
}
