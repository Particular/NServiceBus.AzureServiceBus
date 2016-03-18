namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology
{
    using System;
    using System.Linq;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_publishers
    {
        private PublishersConfiguration _configuration;

        [SetUp]
        public void SetUp()
        {
            var conventions = new FakeConventions();
            _configuration = new PublishersConfiguration(conventions, new SettingsHolder());
        }

        [Test]
        public void Should_throw_if_mapping_does_not_exist()
        {
            var exception = Assert.Throws<InvalidOperationException>(() => _configuration.GetPublishersFor(typeof(MyType)));
            Assert.AreEqual($"No publishers configured for `{typeof(MyType).FullName}`", exception.Message);
        }

        [Test]
        public void Should_not_map_publisher_and_type_if_it_is_not_a_message()
        {
            _configuration.Map("publisher", typeof(MyType));

            var exception = Assert.Throws<InvalidOperationException>(() => _configuration.GetPublishersFor(typeof(MyType)));
            Assert.AreEqual($"No publishers configured for `{typeof(MyType).FullName}`", exception.Message);
        }

        [Test]
        public void Should_map_publisher_and_type_if_it_is_a_message()
        {
            _configuration.Map("publisher", typeof(MyBaseMessage));

            var publishers = _configuration.GetPublishersFor(typeof(MyBaseMessage));

            CollectionAssert.Contains(publishers, "publisher");
        }

        [Test]
        public void Should_map_publishers_for_same_message_type()
        {
            _configuration.Map("publisher1", typeof(MyBaseMessage));
            _configuration.Map("publisher2", typeof(MyBaseMessage));

            var publishers = _configuration.GetPublishersFor(typeof(MyBaseMessage));

            CollectionAssert.Contains(publishers, "publisher1");
            CollectionAssert.Contains(publishers, "publisher2");
        }

        [Test]
        public void Should_map_publishers_for_message_type_and_its_base_class()
        {
            _configuration.Map("publisher", typeof(MyDerivedMessage1));

            var myBaseMessagePublishers = _configuration.GetPublishersFor(typeof(MyBaseMessage));
            var myDerivedMessagePublishers = _configuration.GetPublishersFor(typeof(MyDerivedMessage1));

            CollectionAssert.Contains(myBaseMessagePublishers, "publisher");
            CollectionAssert.Contains(myDerivedMessagePublishers, "publisher");

            Assert.False(_configuration.HasPublishersFor(typeof(MyType)));
        }

        [Test]
        public void Should_map_publishers_for_different_message_types_with_same_base_classes()
        {
            _configuration.Map("publisher1", typeof(MyDerivedMessage1));
            _configuration.Map("publisher2", typeof(MyDerivedMessage2));

            var myBaseMessagePublishers = _configuration.GetPublishersFor(typeof(MyBaseMessage));
            var myDerivedMessage1Publishers = _configuration.GetPublishersFor(typeof(MyDerivedMessage1));
            var myDerivedMessage2Publishers = _configuration.GetPublishersFor(typeof(MyDerivedMessage2));

            CollectionAssert.Contains(myBaseMessagePublishers, "publisher1");
            CollectionAssert.Contains(myBaseMessagePublishers, "publisher2");
            CollectionAssert.Contains(myDerivedMessage1Publishers, "publisher1");
            CollectionAssert.DoesNotContain(myDerivedMessage1Publishers, "publisher2");
            CollectionAssert.Contains(myDerivedMessage2Publishers, "publisher2");
            CollectionAssert.DoesNotContain(myDerivedMessage2Publishers, "publisher1");

            Assert.False(_configuration.HasPublishersFor(typeof(MyType)));
        }

        [Test]
        public void Should_not_map_publisher_twice_for_same_message_type()
        {
            _configuration.Map("publisher", typeof(MyBaseMessage));
            _configuration.Map("publisher", typeof(MyBaseMessage));

            var publishers = _configuration.GetPublishersFor(typeof(MyBaseMessage));

            Assert.AreEqual(1, publishers.Count());
        }

        class MyType { }
        class MyBaseMessage : MyType { }
        class MyDerivedMessage1 : MyBaseMessage { }
        class MyDerivedMessage2 : MyBaseMessage { }
        
        class FakeConventions : IConventions
        {
            public bool IsMessageType(Type type)
            {
                return type != typeof(MyType);
            }
        }
    }

    
}