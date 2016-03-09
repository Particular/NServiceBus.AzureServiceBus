namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Addressing.Composition
{
    using NServiceBus.AzureServiceBus;
    using NServiceBus.AzureServiceBus.Addressing;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_hierarchy_composition_strategy
    {
        [Test]
        public void Hierarchy_composition_will_prefix_entity_name_with_path_for_queues()
        {
            var prefix = "/my/path/";
            var entityname = "myqueue";

            var strategy = new HierarchyCompositionStrategy();
            strategy.SetPathGenerator(s => prefix);

            Assert.AreEqual(prefix + entityname, strategy.GetEntityPath(entityname, EntityType.Queue));
        }

        [Test]
        public void Hierarchy_composition_will_prefix_entity_name_with_path_for_topics()
        {
            var prefix = "/my/path/";
            var entityname = "mytopic";

            var strategy = new HierarchyCompositionStrategy();
            strategy.SetPathGenerator(s => prefix);

            Assert.AreEqual(prefix + entityname, strategy.GetEntityPath(entityname, EntityType.Topic));
        }

        [Test]
        public void Hierarchy_composition_will_not_prefix_entity_name_with_path_for_subscriptions()
        {
            var prefix = "/my/path/";
            var entityname = "myqueue";

            var strategy = new HierarchyCompositionStrategy();
            strategy.SetPathGenerator(s => prefix);

            Assert.AreEqual(entityname, strategy.GetEntityPath(entityname, EntityType.Subscription));
        }

    }
}