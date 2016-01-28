namespace NServiceBus.AzureServiceBus.EnduranceTests
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using NServiceBus.AzureServiceBus.EnduranceTests.Commands;

    internal class Program
    {
        private static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();

            var endpointName = "EnduranceTest";

            Console.WriteLine("CTRL + C to stop test. Press Enter to continue.");
            Console.ReadLine();

            Console.CancelKeyPress += (s, e) =>
            {
                e.Cancel = true;
                cts.Cancel();
            };

            try
            {
                RunEndpointAsync(endpointName, cts.Token).Wait(cts.Token);
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Test Cancelled. Press ENTER to continue.");
                Console.ReadLine();
            }
        }

        private static async Task RunEndpointAsync(string endpointName, CancellationToken ct)
        {
            var busConfiguration = new BusConfiguration();
            busConfiguration.UseTransport<AzureServiceBusTransport>();
            busConfiguration.EndpointName(endpointName);
            busConfiguration.UseSerialization<JsonSerializer>();
            busConfiguration.EnableInstallers();
            busConfiguration.UsePersistence<InMemoryPersistence>();
            busConfiguration.SendFailedMessagesTo("error");
            var endpoint = await Endpoint.Start(busConfiguration);
            Console.WriteLine(endpointName + " started");
            var session = endpoint.CreateBusSession();
            await session.Send<StartHeartbeat>(cmd =>
            {
                cmd.Wait = TestSettings.Rate;
                cmd.TestRunId = Guid.NewGuid();
            }, TestSettings.SendOptions);
            try
            {
                do
                {
                    await Task.Delay(1000, ct);
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
