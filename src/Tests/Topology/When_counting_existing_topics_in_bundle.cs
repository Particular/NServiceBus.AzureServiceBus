namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology
{
    using System;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Microsoft.ServiceBus;
    using Microsoft.ServiceBus.Messaging;
    using NUnit.Framework;
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

            var filter = $"startswith(path, '{bundlePrefix}') eq true";
            var foundTopics = await namespaceManager.GetTopics(filter).ConfigureAwait(false);

            var topicsInBundle = NumberOfTopicsInBundleCheck.CountTopicsInBundle(new Regex($@"^{bundlePrefix}\d+$", RegexOptions.CultureInvariant), foundTopics);
            Assert.AreEqual(0, topicsInBundle);
        }

        [TearDown]
        public Task TearDown()
        {
            return namespaceManager.DeleteTopic(nonBundledTopic);
        }
    }
}
