namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.PreStartupChecks
{
    using System.Linq;
    using System.Threading.Tasks;
    using FakeItEasy;
    using Tests;
    using Transport.AzureServiceBus;
    using Settings;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_executing_NumberOfTopicsInBundleCheck
    {
        [Test]
        public async Task Should_not_throw_if_user_provided_a_custom_namespace_manager()
        {
            var settings = new SettingsHolder();

            var namespaces = new NamespaceConfigurations
            {
                {"name1", ConnectionStringValue.Build("namespace1"), NamespacePurpose.Partitioning},
                {"name2", ConnectionStringValue.Build("namespace2"), NamespacePurpose.Partitioning}
            };
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Namespaces, namespaces);

#pragma warning disable 618
            var namespaceManager = A.Fake<INamespaceManager>();
#pragma warning restore 618
            A.CallTo(() => namespaceManager.CanManageEntities()).Returns(Task.FromResult(true));
#pragma warning disable 618
            var manageNamespaceLifeCycle = A.Fake<IManageNamespaceManagerLifeCycle>();
#pragma warning restore 618
            A.CallTo(() => manageNamespaceLifeCycle.Get(A<string>.Ignored)).Returns(namespaceManager);

            var namespaceBundleConfigurations = await NumberOfTopicsInBundleCheck.Run(manageNamespaceLifeCycle, namespaces, "bundle");

            Assert.That(namespaceBundleConfigurations.Count(), Is.Zero);
        }
    }
}
