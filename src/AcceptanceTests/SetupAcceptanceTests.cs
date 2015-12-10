using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using NUnit.Framework;

/// <summary>
/// Global setup fixture
/// </summary>
[SetUpFixture]
public class SetupAcceptanceTests
{
    [SetUp]
    public void SetUp()
    {
        // case 1:
        // ConfigureTopology = (trans) => (trans as ASB).Topology = Activator.Create(env["topology"]) // for "metadata" about the transport
        // case 2:
        // setting topology for all ATTs
        // setting.Set<TransportDefinition>(topology_to_use)
    }

    private string ConnectionString { get; } = Environment.GetEnvironmentVariable("AzureServiceBusTransport.ConnectionString");

    [TestFixtureTearDown]
    public void TearDown()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var namespaceManager = NamespaceManager.CreateFromConnectionString(ConnectionString);

        var topics = namespaceManager.GetTopics();
        Parallel.ForEach(topics, topic =>
        {
            var subscriptions = namespaceManager.GetSubscriptions(topic.Path);

            Task.WaitAll(subscriptions.Select(x => ClearSubscription(topic.Path, x.Name)).ToArray());
        });

        Task.WaitAll(namespaceManager.GetQueues().Select(q => ClearQueue(q.Path)).ToArray());

        stopwatch.Stop();
        Console.WriteLine("Time to cleanup: {0} seconds", TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds).TotalSeconds);
    }

    const int BATCH_SIZE_FOR_CLEARING = 1000;
    const int SECONDS_TO_WAIT_FOR_BATCH = 10;

    async Task ClearQueue(string queuePath)
    {
        var client = QueueClient.CreateFromConnectionString(ConnectionString, queuePath, ReceiveMode.ReceiveAndDelete);
        IEnumerable<BrokeredMessage> messages;
        do
        {
            messages = await client.ReceiveBatchAsync(BATCH_SIZE_FOR_CLEARING, TimeSpan.FromSeconds(SECONDS_TO_WAIT_FOR_BATCH));
        } while (messages.Any());

        Console.WriteLine("Cleared '{0}' queue", queuePath);
    }

    async Task ClearSubscription(string topicPath, string name)
    {
        var client = SubscriptionClient.CreateFromConnectionString(ConnectionString, topicPath, name, ReceiveMode.ReceiveAndDelete);
        IEnumerable<BrokeredMessage> messages;
        do
        {
            messages = await client.ReceiveBatchAsync(BATCH_SIZE_FOR_CLEARING, TimeSpan.FromSeconds(SECONDS_TO_WAIT_FOR_BATCH));
        } while (messages.Any());

        Console.WriteLine("Cleared '{0}->{1}' subscription", topicPath, name);
    }
}

