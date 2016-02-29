namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.PreStartupChecks
{
    using System.Threading.Tasks;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_executing_topic_partitioning_check_for_forwarding_topology
    {
        [Test]
        public async Task Should_succeed_when_default_settings_are_applied()
        {
            var settings = new SettingsHolder();
            new DefaultConfigurationValues().Apply(settings);

            var check = new TopicPartitioningCheckForForwardingTopology(settings);
            var result = await check.Run();

            Assert.IsTrue(result.Succeeded, "Result was expected to be successful, but it wasn't.");
        }

        [Test]
        public async void Should_return_failed_result_for_enabled_topic_partitioning()
        {
            var settings = new SettingsHolder();
            new DefaultConfigurationValues().Apply(settings);
            var topicSettings = new AzureServiceBusTopicSettings(settings);
            topicSettings.EnablePartitioning(true);

            var check = new TopicPartitioningCheckForForwardingTopology(settings);
            var result = await check.Run();

            Assert.IsFalse(result.Succeeded, "Result was expected to be failed, but it wasn't.");
            Assert.IsNotNullOrEmpty(result.ErrorMessage, "Expected to contain an error message.");
        }
    }
}