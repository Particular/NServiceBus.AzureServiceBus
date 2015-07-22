namespace NServiceBus.AzureServiceBus.Tests
{
    using NServiceBus.AzureServiceBus.Addressing;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_using_flat_composition_strategy
    {
        [Test]
        public void Flat_composition_will_just_return_entityname()
        {
            var strategy = new FlatCompositionStrategy();
            var entityname = "myqueue";

            Assert.AreEqual(entityname, strategy.GetEntityPath(entityname));
        }

    }
}