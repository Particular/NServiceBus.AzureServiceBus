namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.PreStartupChecks
{
    using System.Threading.Tasks;
    using AzureServiceBus.Topology.MetaModel;
    using FakeItEasy;
    using Tests;
    using Transport.AzureServiceBus;
    using NUnit.Framework;

#pragma warning disable 618
    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_execute_manage_rights_startup_check
    {
        [Test]
        public async Task Should_return_no_namespaces_when_all_namespaces_have_manage_rights()
        {
            var namespaceConfigurations = new NamespaceConfigurations
            {
                {"name1", ConnectionStringValue.Build("namespace1"), NamespacePurpose.Partitioning},
                {"name2", ConnectionStringValue.Build("namespace2"), NamespacePurpose.Partitioning}
            };

            var namespaceManager = A.Fake<INamespaceManagerInternal>();
            A.CallTo(() => namespaceManager.CanManageEntities()).Returns(Task.FromResult(true));
            var manageNamespaceLifeCycle = A.Fake<IManageNamespaceManagerLifeCycleInternal>();
            A.CallTo(() => manageNamespaceLifeCycle.Get(A<string>.Ignored)).Returns(namespaceManager);

            var result = await ManageRightsCheck.Run(manageNamespaceLifeCycle, namespaceConfigurations);

            CollectionAssert.IsEmpty(result);
        }

        [Test]
        public async Task Should_namespaces_that_dont_have_manage_rights()
        {
            var namespaceConfigurations = new NamespaceConfigurations
            {
                {"name1", ConnectionStringValue.Build("namespace1"), NamespacePurpose.Partitioning},
                {"name2", ConnectionStringValue.Build("namespace2"), NamespacePurpose.Partitioning}
            };

            var trueNamespaceManager = A.Fake<INamespaceManagerInternal>();
            A.CallTo(() => trueNamespaceManager.CanManageEntities()).Returns(Task.FromResult(true));
            var falseNamespaceManager = A.Fake<INamespaceManagerInternal>();
            A.CallTo(() => falseNamespaceManager.CanManageEntities()).Returns(Task.FromResult(false));
            var manageNamespaceLifeCycle = A.Fake<IManageNamespaceManagerLifeCycleInternal>();
            A.CallTo(() => manageNamespaceLifeCycle.Get("name1")).Returns(trueNamespaceManager);
            A.CallTo(() => manageNamespaceLifeCycle.Get("name2")).Returns(falseNamespaceManager);

            var result = await ManageRightsCheck.Run(manageNamespaceLifeCycle, namespaceConfigurations);

            CollectionAssert.Contains(result, "name2");
            CollectionAssert.DoesNotContain(result, "name1");
        }
    }
}
