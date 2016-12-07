namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Sending
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AzureServiceBus;
    using Tests;
    using Transport.AzureServiceBus;
    using DeliveryConstraints;
    using Routing;
    using Settings;
    using Transport;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_batching_TransportOperations
    {
        [Test]
        public void Should_not_batch_different_types_of_transport_operations_together()
        {
            var headers = new Dictionary<string, string>();
            var body = new byte[0];
            var deliveryConstraints = new List<DeliveryConstraint>();

            var operation1 = new TransportOperation(new OutgoingMessage("id-1", headers, body), new MulticastAddressTag(typeof(EventA)), DispatchConsistency.Default, deliveryConstraints);
            var operation2 = new TransportOperation(new OutgoingMessage("id-2", headers, body), new UnicastAddressTag("CommandA"), DispatchConsistency.Default, deliveryConstraints);

            var transportOperations = new TransportOperations(operation1, operation2);

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var batcher = new Batcher(new FakeTopolySectionManager(), settings);
            var batches = batcher.ToBatches(transportOperations);

            Assert.That(batches.Count, Is.EqualTo(2));
            Assert.That(batches[0].Operations.Count, Is.EqualTo(1));
            Assert.That(batches[1].Operations.Count, Is.EqualTo(1));
        }

        [Test]
        public void Should_batch_transport_operations_of_the_same_type_with_the_same_dispatch_consistency()
        {
            var headers = new Dictionary<string, string>();
            var body = new byte[0];
            var deliveryConstraints = new List<DeliveryConstraint>();

            var operation1 = new TransportOperation(new OutgoingMessage("id-1", headers, body), new MulticastAddressTag(typeof(EventA)), DispatchConsistency.Default, deliveryConstraints);
            var operation2 = new TransportOperation(new OutgoingMessage("id-2", headers, body), new MulticastAddressTag(typeof(EventA)), DispatchConsistency.Default, deliveryConstraints);
            var operation3 = new TransportOperation(new OutgoingMessage("id-3", headers, body), new UnicastAddressTag("CommandA"), DispatchConsistency.Isolated, deliveryConstraints);
            var operation4 = new TransportOperation(new OutgoingMessage("id-4", headers, body), new UnicastAddressTag("CommandA"), DispatchConsistency.Isolated);

            var transportOperations = new TransportOperations(operation1, operation2, operation3, operation4);

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var batcher = new Batcher(new FakeTopolySectionManager(), settings);
            var batches = batcher.ToBatches(transportOperations);

            Assert.That(batches.Count, Is.EqualTo(2));
            Assert.That(batches[0].Operations.Count, Is.EqualTo(2));
            Assert.That(batches[1].Operations.Count, Is.EqualTo(2));
        }

        [Test]
        public void Should_not_batch_transport_operations_of_the_same_type_with_different_dispatch_consistency()
        {
            var headers = new Dictionary<string, string>();
            var body = new byte[0];
            var deliveryConstraints = new List<DeliveryConstraint>();

            var operation1 = new TransportOperation(new OutgoingMessage("id-1", headers, body), new MulticastAddressTag(typeof(EventA)) , DispatchConsistency.Default, deliveryConstraints);
            var operation2 = new TransportOperation(new OutgoingMessage("id-2", headers, body), new MulticastAddressTag(typeof(EventA)), DispatchConsistency.Isolated, deliveryConstraints);
            var operation3 = new TransportOperation(new OutgoingMessage("id-3", headers, body), new UnicastAddressTag("CommandA"), DispatchConsistency.Default, deliveryConstraints);
            var operation4 = new TransportOperation(new OutgoingMessage("id-4", headers, body), new UnicastAddressTag("CommandA"), DispatchConsistency.Isolated, deliveryConstraints);

            var transportOperations = new TransportOperations(operation1, operation2, operation3, operation4);

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var batcher = new Batcher(new FakeTopolySectionManager(), settings);
            var batches = batcher.ToBatches(transportOperations);

            Assert.That(batches.Count, Is.EqualTo(4));
            Assert.That(batches[0].Operations.First().Message.MessageId, Is.EqualTo("id-1"));
            Assert.That(batches[1].Operations.First().Message.MessageId, Is.EqualTo("id-2"));
            Assert.That(batches[2].Operations.First().Message.MessageId, Is.EqualTo("id-3"));
            Assert.That(batches[3].Operations.First().Message.MessageId, Is.EqualTo("id-4"));
        }

        [Test]
        public void Should_calculate_size_of_each_batched_operation()
        {
            var headers = new Dictionary<string, string>{ { "header", "value"} };
            var body = new byte[100];
            var deliveryConstraints = new List<DeliveryConstraint>();

            var operation1 = new TransportOperation(new OutgoingMessage("id-1", headers, body), new MulticastAddressTag(typeof(EventA)), DispatchConsistency.Default, deliveryConstraints);
            var operation2 = new TransportOperation(new OutgoingMessage("id-2", headers, body), new UnicastAddressTag("CommandA"), DispatchConsistency.Default, deliveryConstraints);

            var transportOperations = new TransportOperations(operation1, operation2);

            var settings = new DefaultConfigurationValues().Apply(new SettingsHolder());

            var batcher = new Batcher(new FakeTopolySectionManager(), settings);
            var batches = batcher.ToBatches(transportOperations);

            Assert.That(batches.Count, Is.EqualTo(2));
            Assert.That(batches[0].Operations.First().GetEstimatedSize(), Is.EqualTo(2088), "For default message size padding of 5% size should be 2,088 bytes");
            Assert.That(batches[1].Operations.First().GetEstimatedSize(), Is.EqualTo(2088), "For default message size padding of 5% size should be 2,088 bytes");
        }

    }

    public class EventA
    {
    }
    public class EventB
    {
    }

    class FakeTopolySectionManager : ITopologySectionManagerInternal
    {
        public TopologySectionInternal DetermineReceiveResources(string inputQueue)
        {
            throw new NotImplementedException();
        }

        public TopologySectionInternal DetermineResourcesToCreate(QueueBindings queueBindings)
        {
            throw new NotImplementedException();
        }

        public TopologySectionInternal DeterminePublishDestination(Type eventType)
        {
            return new TopologySectionInternal
            {
                Namespaces = new List<RuntimeNamespaceInfo> { new RuntimeNamespaceInfo("name", ConnectionStringValue.Sample) },
                Entities = new List<EntityInfoInternal> { new EntityInfoInternal() }
            };
        }

        public TopologySectionInternal DetermineSendDestination(string destination)
        {
            return new TopologySectionInternal
            {
                Namespaces = new List<RuntimeNamespaceInfo> { new RuntimeNamespaceInfo("name", ConnectionStringValue.Sample) },
                Entities = new List<EntityInfoInternal> { new EntityInfoInternal() }
            };

        }

        public TopologySectionInternal DetermineResourcesToSubscribeTo(Type eventType)
        {
            return new TopologySectionInternal
            {
                Namespaces = new List<RuntimeNamespaceInfo> { new RuntimeNamespaceInfo("name", ConnectionStringValue.Sample) },
                Entities = new List<EntityInfoInternal> { new EntityInfoInternal() }
            };
        }

        public TopologySectionInternal DetermineResourcesToUnsubscribeFrom(Type eventtype)
        {
            throw new NotImplementedException();
        }
    }
}