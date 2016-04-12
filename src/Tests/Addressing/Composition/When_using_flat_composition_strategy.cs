namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Addressing.Composition
{
    using AzureServiceBus;
    using AzureServiceBus.Addressing;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_flat_composition_strategy
    {
        [Test]
        public void Flat_composition_will_just_return_entityname_for_queues()
        {
            var strategy = new FlatComposition();
            var entityname = "myqueue";

            Assert.AreEqual(entityname, strategy.GetEntityPath(entityname, EntityType.Queue));
        }

        [Test]
        public void Flat_composition_will_just_return_entityname_for_topics()
        {
            var strategy = new FlatComposition();
            var entityname = "mytopic";

            Assert.AreEqual(entityname, strategy.GetEntityPath(entityname, EntityType.Topic));
        }

        [Test]
        public void Flat_composition_will_just_return_entityname_for_subscriptions()
        {
            var strategy = new FlatComposition();
            var entityname = "mysub";

            Assert.AreEqual(entityname, strategy.GetEntityPath(entityname, EntityType.Subscription));
        }

    }

}