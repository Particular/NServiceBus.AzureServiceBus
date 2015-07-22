namespace NServiceBus.AzureServiceBus.Tests
{
    using NServiceBus.AzureServiceBus.Addressing;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_hierarchy_composition_strategy
    {
        [Test]
        public void Hierarchy_composition_will_prefix_entity_name_with_path()
        {
            var prefix = "/my/path/";
            var entityname = "myqueue";

            var strategy = new HierarchyCompositionStrategy();
            strategy.SetPathGenerator(s => prefix);

            Assert.AreEqual(prefix + entityname, strategy.GetEntityPath(entityname));
        }

    }
}