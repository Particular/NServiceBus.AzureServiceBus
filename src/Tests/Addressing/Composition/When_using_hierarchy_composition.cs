namespace NServiceBus.AzureServiceBus.Tests
{
    using NServiceBus.AzureServiceBus.Addressing;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_hierarchy_composition
    {
        [Test]
        public void Hierarchy_composition_will_prefix_entityname()
        {
            var prefix = "/my/path/";
            var entityname = "myqueue";

            var strategy = new HierarchyCompositionStrategy();
            strategy.SetPathGenerator(s => prefix);

            Assert.AreEqual(prefix + entityname, strategy.GetEntityPath(entityname));
        }

    }
}