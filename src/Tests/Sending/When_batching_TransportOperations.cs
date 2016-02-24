namespace NServiceBus.AzureServiceBus.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NServiceBus.DeliveryConstraints;
    using NServiceBus.Routing;
    using NServiceBus.Settings;
    using NServiceBus.Transports;
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
        public void Should_batch_transport_operations_of_the_same_type_with_the_same_dispatch_consitency()
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
        public void Should_not_batch_transport_operations_of_the_same_type_with_different_dispatch_consitency()
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
            Assert.That(batches[0].Operations.First().GetEstimatedSize(), Is.EqualTo(2164), "For default message size padding of 5% size should be 2,164 bytes");
            Assert.That(batches[1].Operations.First().GetEstimatedSize(), Is.EqualTo(2164), "For default message size padding of 5% size should be 2,164 bytes");
        }

    }

    public class EventA
    {
    }
    public class EventB
    {
    }

    public class FakeTopolySectionManager : ITopologySectionManager
    {
        public TopologySection DetermineReceiveResources(string inputQueue)
        {
            throw new NotImplementedException();
        }

        public TopologySection DetermineResourcesToCreate()
        {
            throw new NotImplementedException();
        }

        public TopologySection DeterminePublishDestination(Type eventType)
        {
            return new TopologySection
            {
                Namespaces = new List<RuntimeNamespaceInfo> { new RuntimeNamespaceInfo("name", "connectionString") },
                Entities = new List<EntityInfo> { new EntityInfo() }
            };
        }

        public TopologySection DetermineSendDestination(string destination)
        {
            return new TopologySection
            {
                Namespaces = new List<RuntimeNamespaceInfo> { new RuntimeNamespaceInfo("name", "connectionString") },
                Entities = new List<EntityInfo> { new EntityInfo() }
            };

        }

        public TopologySection DetermineResourcesToSubscribeTo(Type eventType)
        {
            throw new NotImplementedException();
        }

        public TopologySection DetermineResourcesToUnsubscribeFrom(Type eventtype)
        {
            throw new NotImplementedException();
        }
    }
}