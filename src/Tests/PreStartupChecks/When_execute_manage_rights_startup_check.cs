namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.PreStartupChecks
{
    using System.Threading.Tasks;
    using FakeItEasy;
    using Tests;
    using AzureServiceBus;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_execute_manage_rights_startup_check
    {
        [Test]
        public async void Should_return_success_if_create_queues_is_not_required()
        {
            var settings = new SettingsHolder();
            settings.Set(WellKnownConfigurationKeys.Core.CreateTopology, false);

            var check = new ManageRightsCheck(A.Fake<IManageNamespaceManagerLifeCycle>(), settings);
            var result = await check.Run();

            Assert.True(result.Succeeded);
        }

        [Test]
        public async void Should_return_success_if_all_namespaces_have_manage_rights()
        {
            var settings = new SettingsHolder();
            settings.Set(WellKnownConfigurationKeys.Core.CreateTopology, true);

            var namespaces = new NamespaceConfigurations
            {
                {"name1", ConnectionStringValue.Build("namespace1")},
                {"name2", ConnectionStringValue.Build("namespace2")}
            };
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, namespaces);

            var namespaceManager = A.Fake<INamespaceManager>();
            A.CallTo(() => namespaceManager.CanManageEntities()).Returns(Task.FromResult(true));
            var manageNamespaceLifeCycle = A.Fake<IManageNamespaceManagerLifeCycle>();
            A.CallTo(() => manageNamespaceLifeCycle.Get(A<string>.Ignored)).Returns(namespaceManager);

            var check = new ManageRightsCheck(manageNamespaceLifeCycle, settings);
            var result = await check.Run();

            Assert.True(result.Succeeded);
        }

        [Test]
        public async void Should_return_failure_if_a_namespace_has_not_manage_rights()
        {
            var settings = new SettingsHolder();
            settings.Set(WellKnownConfigurationKeys.Core.CreateTopology, true);

            var namespaces = new NamespaceConfigurations
            {
                {"name1", ConnectionStringValue.Build("namespace1")},
                {"name2", ConnectionStringValue.Build("namespace2")}
            };
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, namespaces);

            var trueNamespaceManager = A.Fake<INamespaceManager>();
            A.CallTo(() => trueNamespaceManager.CanManageEntities()).Returns(Task.FromResult(true));
            var falseNamespaceManager = A.Fake<INamespaceManager>();
            A.CallTo(() => falseNamespaceManager.CanManageEntities()).Returns(Task.FromResult(false));
            var manageNamespaceLifeCycle = A.Fake<IManageNamespaceManagerLifeCycle>();
            A.CallTo(() => manageNamespaceLifeCycle.Get("name1")).Returns(trueNamespaceManager);
            A.CallTo(() => manageNamespaceLifeCycle.Get("name2")).Returns(falseNamespaceManager);

            var check = new ManageRightsCheck(manageNamespaceLifeCycle, settings);
            var result = await check.Run();

            Assert.False(result.Succeeded);
        }

        [Test]
        public async void Should_compose_right_error_message_when_failed()
        {
            var settings = new SettingsHolder();
            settings.Set(WellKnownConfigurationKeys.Core.CreateTopology, true);

            var namespaces = new NamespaceConfigurations
            {
                {"name1", ConnectionStringValue.Build("namespace1")},
                {"name2", ConnectionStringValue.Build("namespace2")},
                {"name3", ConnectionStringValue.Build("namespace3")},
            };
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Partitioning.Namespaces, namespaces);

            var trueNamespaceManager = A.Fake<INamespaceManager>();
            A.CallTo(() => trueNamespaceManager.CanManageEntities()).Returns(Task.FromResult(true));
            var falseNamespaceManager = A.Fake<INamespaceManager>();
            A.CallTo(() => falseNamespaceManager.CanManageEntities()).Returns(Task.FromResult(false));
            var manageNamespaceLifeCycle = A.Fake<IManageNamespaceManagerLifeCycle>();
            A.CallTo(() => manageNamespaceLifeCycle.Get("name1")).Returns(trueNamespaceManager);
            A.CallTo(() => manageNamespaceLifeCycle.Get("name2")).Returns(falseNamespaceManager);
            A.CallTo(() => manageNamespaceLifeCycle.Get("name3")).Returns(falseNamespaceManager);

            var check = new ManageRightsCheck(manageNamespaceLifeCycle, settings);
            var result = await check.Run();

            StringAssert.DoesNotContain("name1", result.ErrorMessage);
            StringAssert.Contains("name2", result.ErrorMessage);
            StringAssert.Contains("name3", result.ErrorMessage);
        }
    }
}
