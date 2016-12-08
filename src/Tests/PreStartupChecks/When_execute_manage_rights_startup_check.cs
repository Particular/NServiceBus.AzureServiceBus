namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.PreStartupChecks
{
    using System.Threading.Tasks;
    using FakeItEasy;
    using Tests;
    using Transport.AzureServiceBus;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_execute_manage_rights_startup_check
    {
        [Test]
        public async Task Should_return_success_if_all_namespaces_have_manage_rights()
        {
            var settings = new SettingsHolder();

            var namespaces = new NamespaceConfigurations
            {
                {"name1", ConnectionStringValue.Build("namespace1"), NamespacePurpose.Partitioning},
                {"name2", ConnectionStringValue.Build("namespace2"), NamespacePurpose.Partitioning}
            };
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, namespaces);

            var namespaceManager = A.Fake<INamespaceManagerInternal>();
            A.CallTo(() => namespaceManager.CanManageEntities()).Returns(Task.FromResult(true));
            var manageNamespaceLifeCycle = A.Fake<IManageNamespaceManagerLifeCycleInternal>();
            A.CallTo(() => manageNamespaceLifeCycle.Get(A<string>.Ignored)).Returns(namespaceManager);

            var check = new ManageRightsCheck(manageNamespaceLifeCycle, settings);
            var result = await check.Run();

            Assert.True(result.Succeeded);
        }

        [Test]
        public async Task Should_return_failure_if_a_namespace_has_not_manage_rights()
        {
            var settings = new SettingsHolder();

            var namespaces = new NamespaceConfigurations
            {
                {"name1", ConnectionStringValue.Build("namespace1"), NamespacePurpose.Partitioning},
                {"name2", ConnectionStringValue.Build("namespace2"), NamespacePurpose.Partitioning}
            };
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, namespaces);

            var trueNamespaceManager = A.Fake<INamespaceManagerInternal>();
            A.CallTo(() => trueNamespaceManager.CanManageEntities()).Returns(Task.FromResult(true));
            var falseNamespaceManager = A.Fake<INamespaceManagerInternal>();
            A.CallTo(() => falseNamespaceManager.CanManageEntities()).Returns(Task.FromResult(false));
            var manageNamespaceLifeCycle = A.Fake<IManageNamespaceManagerLifeCycleInternal>();
            A.CallTo(() => manageNamespaceLifeCycle.Get("name1")).Returns(trueNamespaceManager);
            A.CallTo(() => manageNamespaceLifeCycle.Get("name2")).Returns(falseNamespaceManager);

            var check = new ManageRightsCheck(manageNamespaceLifeCycle, settings);
            var result = await check.Run();

            Assert.False(result.Succeeded);
        }

        [Test]
        public async Task Should_compose_right_error_message_when_failed()
        {
            var settings = new SettingsHolder();

            var namespaces = new NamespaceConfigurations
            {
                {"name1", ConnectionStringValue.Build("namespace1"), NamespacePurpose.Partitioning},
                {"name2", ConnectionStringValue.Build("namespace2"), NamespacePurpose.Partitioning},
                {"name3", ConnectionStringValue.Build("namespace3"), NamespacePurpose.Partitioning},
            };
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, namespaces);

            var trueNamespaceManager = A.Fake<INamespaceManagerInternal>();
            A.CallTo(() => trueNamespaceManager.CanManageEntities()).Returns(Task.FromResult(true));
            var falseNamespaceManager = A.Fake<INamespaceManagerInternal>();
            A.CallTo(() => falseNamespaceManager.CanManageEntities()).Returns(Task.FromResult(false));
            var manageNamespaceLifeCycle = A.Fake<IManageNamespaceManagerLifeCycleInternal>();
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
