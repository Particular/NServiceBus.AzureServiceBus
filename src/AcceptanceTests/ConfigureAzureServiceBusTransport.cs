using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using NServiceBus;
using NServiceBus.Configuration.AdvanceExtensibility;
using NServiceBus.Transports;

public class ConfigureAzureServiceBusTransport
{
    const int BATCH_SIZE_FOR_CLEARING = 1000;
    const int SECONDS_TO_WAIT_FOR_BATCH = 10;

    BusConfiguration busConfiguration;
    private string ConnectionString { get; } = Environment.GetEnvironmentVariable("AzureServiceBusTransport.ConnectionString");

    public Task Configure(BusConfiguration configuration)
    {
        busConfiguration = configuration;
        return Task.FromResult(0);
    }

    public Task Cleanup()
    {
        var bindings = busConfiguration.GetSettings().Get<QueueBindings>();

        var namespaceManager = NamespaceManager.CreateFromConnectionString(ConnectionString);

        var topicPaths = bindings.ReceivingAddresses.Select(x => x + ".events");

        Parallel.ForEach(topicPaths, topicPath =>
        {
            var subscriptions = namespaceManager.GetSubscriptions(topicPath);

            Task.WaitAll(subscriptions.Select(x => ClearSubscription(topicPath, x.Name)).Union(subscriptions.Select(x => ClearSubscription(topicPath,x.Name + "/$DeadLetterQueue"))).ToArray());
        });

        var queues = bindings.ReceivingAddresses.Union(bindings.SendingAddresses).Union(bindings.ReceivingAddresses.Select(QueueClient.FormatDeadLetterPath)).Union(bindings.SendingAddresses.Select(QueueClient.FormatDeadLetterPath));
        Task.WaitAll(queues.Select(ClearQueue).ToArray());

        return Task.FromResult(0);
    }

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