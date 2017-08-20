namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Addressing.Composition
{
    using System;
    using NUnit.Framework;
    using Settings;
    using Transport.AzureServiceBus;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_hierarchy_composition_strategy
    {
        [Test]
        public void Hierarchy_composition_will_prefix_entity_name_with_path_for_queues()
        {
            var prefix = "/my/path/";
            var entityname = "myqueue";

            var settings = new SettingsHolder();
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Composition.HierarchyCompositionPathGenerator, (Func<string, string>)(s => prefix));
            var strategy = new HierarchyComposition();
            strategy.Initialize(settings);

            Assert.AreEqual(prefix + entityname, strategy.GetEntityPath(entityname, EntityType.Queue));
        }

        [Test]
        public void Hierarchy_composition_will_prefix_entity_name_with_path_for_topics()
        {
            var prefix = "/my/path/";
            var entityname = "mytopic";

            var settings = new SettingsHolder();
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Composition.HierarchyCompositionPathGenerator, (Func<string, string>)(s => prefix));
            var strategy = new HierarchyComposition();
            strategy.Initialize(settings);

            Assert.AreEqual(prefix + entityname, strategy.GetEntityPath(entityname, EntityType.Topic));
        }

        [Test]
        public void Hierarchy_composition_will_not_prefix_entity_name_with_path_for_subscriptions()
        {
            var prefix = "/my/path/";
            var entityname = "myqueue";

            var settings = new SettingsHolder();
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Composition.HierarchyCompositionPathGenerator, (Func<string, string>)(s => prefix));
            var strategy = new HierarchyComposition();
            strategy.Initialize(settings);

            Assert.AreEqual(entityname, strategy.GetEntityPath(entityname, EntityType.Subscription));
        }

        [Test]
        public void Hierarchy_composition_will_not_prefix_entity_name_with_path_for_rules()
        {
            var prefix = "/my/path/";
            var entityname = "myrule";

            var settings = new SettingsHolder();
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Composition.HierarchyCompositionPathGenerator, (Func<string, string>)(s => prefix));
            var strategy = new HierarchyComposition();
            strategy.Initialize(settings);

            Assert.AreEqual(entityname, strategy.GetEntityPath(entityname, EntityType.Rule));
        }
    }
}