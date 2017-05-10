namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology.MetaModel
{
    using System;
    using AzureServiceBus.Topology.MetaModel;
    using NUnit.Framework;

#pragma warning disable 618
    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_namespace_bundle_configurations_is_used
    {
        NamespaceBundleConfigurations namespaceBundleConfigurations;

        [SetUp]
        public void SetUp()
        {
            namespaceBundleConfigurations = new NamespaceBundleConfigurations();
        }

        [Test]
        [TestCase("alias", 3)]
        [TestCase("ALIAS", 3)]
        [TestCase("Alias", 3)]
        public void Should_throw_if_namespace_alias_is_added_again_with_a_smaller_number_of_topics_in_bundle(string name, int numberOfTopicsInBundle)
        {
            namespaceBundleConfigurations.Add(name, 2);
            Assert.Throws<Exception>(() => namespaceBundleConfigurations.Add(name, numberOfTopicsInBundle));
        }

        [Test]
        public void Should_get_number_of_bundles_for_a_registered_alias()
        {
            namespaceBundleConfigurations.Add("alias", 5);
            var value = namespaceBundleConfigurations.GetNumberOfTopicInBundle("AlIaS");

            Assert.That(value, Is.EqualTo(5));
        }

        [Test]
        public void Should_return_zero_for_a_namespace_that_is_not_found()
        {
            var value = namespaceBundleConfigurations.GetNumberOfTopicInBundle("alias");
            Assert.That(value, Is.EqualTo(1));
        }
    }
}