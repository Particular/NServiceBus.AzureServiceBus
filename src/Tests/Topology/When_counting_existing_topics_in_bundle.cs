namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NUnit.Framework;
    using Settings;
    using TestUtils;
    using Transport.AzureServiceBus;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_counting_existing_topics_in_bundle
    {
        NamespaceManagerAdapterInternal namespaceManager;
        string nonBundledTopic;

        [Test]
        public async Task Should_only_count_topics_following_bundle_pattern()
        {
            var bundlePrefix = $"bundle{DateTime.Now.Ticks}-";
            nonBundledTopic = $"{bundlePrefix}x";

            namespaceManager = new NamespaceManagerAdapterInternal(NamespaceManager.CreateFromConnectionString(AzureServiceBusConnectionString.Value));

            try
            {
                await namespaceManager.CreateTopic(new TopicDescription(nonBundledTopic));
            }
            catch (MessagingEntityAlreadyExistsException)
            {
                // ignore if topic already exists
            }

            var settings = new SettingsHolder();
            var namespaceConfigurations = new NamespaceConfigurations();
            var namespaceAlias = "namespace1";
            namespaceConfigurations.Add(namespaceAlias, AzureServiceBusConnectionString.Value, NamespacePurpose.Routing);
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, namespaceConfigurations);

            var result = await NumberOfTopicsInBundleCheck.Run(new NamespaceManagerLifeCycleManagerInternal(new NamespaceManagerCreator(settings)), namespaceConfigurations, bundlePrefix);
            Assert.AreEqual(0, result.GetNumberOfTopicInBundle(namespaceAlias));
        }

        [TearDown]
        public Task TearDown() => namespaceManager.DeleteTopic(nonBundledTopic);
    }
}
