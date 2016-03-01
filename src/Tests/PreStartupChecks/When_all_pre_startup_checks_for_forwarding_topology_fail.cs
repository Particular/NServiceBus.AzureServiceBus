namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.PreStartupChecks
{
    using System;
    using System.Threading.Tasks;
    using FakeItEasy;
    using NServiceBus.AzureServiceBus;
    using NServiceBus.Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_all_pre_startup_checks_for_forwarding_topology_fail
    {
        [Test]
        public async Task Should_return_error_message_with_all_errors_listed()
        {
            var settings = new SettingsHolder();
            var container = new TransportPartsContainer();

            settings.Set(WellKnownConfigurationKeys.Core.CreateTopology, true);
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, new NamespaceConfigurations { {"namespace1", "namespace1-connString"} });
            settings.Set(WellKnownConfigurationKeys.Topology.Resources.Topics.EnablePartitioning, true);

            container.Register(typeof(SettingsHolder), () => settings);

            var namespaceManager = A.Fake<INamespaceManager>();
            A.CallTo(() => namespaceManager.CanManageEntities()).Returns(Task.FromResult(false));
            var manageNamespaceLifeCycle = A.Fake<IManageNamespaceManagerLifeCycle>();
            A.CallTo(() => manageNamespaceLifeCycle.Get("namespace1")).Returns(namespaceManager);
            container.Register<IManageNamespaceManagerLifeCycle>(() => manageNamespaceLifeCycle);

            var topology = new ForwardingTopology(container);
            topology.Initialize(settings);

            var result = await topology.RunPreStartupChecks();

            Assert.That(result.ErrorMessage, Is.StringContaining("Configured to create topology, but have no manage rights for the following namespace(s)"));
            Assert.That(result.ErrorMessage, Is.StringContaining("topic partitioning should be disabled. Disable topic partitioning by removing"));
            Console.WriteLine(result.ErrorMessage);
        }
    }
}