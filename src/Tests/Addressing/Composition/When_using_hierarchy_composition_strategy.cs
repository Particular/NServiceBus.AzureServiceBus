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
        const string Prefix = "my/path/";
        HierarchyComposition strategy;

        [SetUp]
        public void Setup()
        {
            var settings = new SettingsHolder();
            settings.Set(WellKnownConfigurationKeys.Topology.Addressing.Composition.HierarchyCompositionPathGenerator, (Func<string, string>)(s => Prefix));
            strategy = new HierarchyComposition(settings);
        }

        [TestCase("myQueue", EntityType.Queue)]
        [TestCase("myTopic", EntityType.Topic)]
        public void Hierarchy_composition_will_prefix_entity_name_with_path_for_queues_and_topics(string entityPath, EntityType entityType) => Assert.AreEqual($"{Prefix}{entityPath}", strategy.GetEntityPath(entityPath, entityType));

        [TestCase("my/path/myQueue", EntityType.Queue)]
        [TestCase("my/path/myTopic", EntityType.Topic)]
        public void Hierarchy_composition_will_not_prefix_entity_name_if_prefix_is_already_applied_to_queues_and_topics(string entityPath, EntityType entityType) => Assert.AreEqual(entityPath, strategy.GetEntityPath(entityPath, entityType));


        [TestCase("mySubscription", EntityType.Subscription)]
        [TestCase("myRule", EntityType.Rule)]
        public void Hierarchy_composition_will_not_prefix_entity_name_with_path_for_subscriptions_and_rules(string entityName, EntityType entityType) => Assert.AreEqual(entityName, strategy.GetEntityPath(entityName, entityType));
    }
}