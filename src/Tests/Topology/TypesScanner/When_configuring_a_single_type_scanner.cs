namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology.EventsScanner
{
    using NServiceBus.AzureServiceBus.TypesScanner;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_a_single_type_scanner
    {
        [Test]
        public void Two_scanners_should_be_equal_if_they_reference_the_same_type()
        {
            var scanner1 = new SingleTypeScanner(typeof(MyType1));
            var scanner2 = new SingleTypeScanner(typeof(MyType1));

            Assert.AreEqual(scanner1, scanner2);

            var hashCode1 = scanner1.GetHashCode();
            var hashCode2 = scanner2.GetHashCode();

            Assert.AreEqual(hashCode1, hashCode2);
        }

        [Test]
        public void Two_scanners_should_be_not_equal_if_they_reference_two_differents_types()
        {
            var scanner1 = new SingleTypeScanner(typeof(MyType1));
            var scanner2 = new SingleTypeScanner(typeof(MyType2));

            Assert.AreNotEqual(scanner1, scanner2);

            var hashCode1 = scanner1.GetHashCode();
            var hashCode2 = scanner2.GetHashCode();

            Assert.AreNotEqual(hashCode1, hashCode2);
        }

        class MyType1 { }

        class MyType2 { }
    }
}