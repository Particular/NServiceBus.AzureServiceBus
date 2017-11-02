namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology.EventsScanner
{
    using System;
    using System.Reflection;
    using Transport.AzureServiceBus;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_an_assembly_scanner
    {
        [Test]
        public void Two_scanners_should_be_equal_if_they_reference_the_same_assembly()
        {
            var scanner1 = new AssemblyTypesScanner(Assembly.GetExecutingAssembly());
            var scanner2 = new AssemblyTypesScanner(Assembly.GetExecutingAssembly());

            Assert.AreEqual(scanner1, scanner2);

            var hashCode1 = scanner1.GetHashCode();
            var hashCode2 = scanner2.GetHashCode();

            Assert.AreEqual(hashCode1, hashCode2);
        }

        [Test]
        public void Two_scanners_should_be_not_equal_if_they_reference_two_different_assemblies()
        {
            var scanner1 = new AssemblyTypesScanner(Assembly.GetExecutingAssembly());
            var scanner2 = new AssemblyTypesScanner(typeof(AzureServiceBusTransport).Assembly);

            Assert.AreNotEqual(scanner1, scanner2);

            var hashCode1 = scanner1.GetHashCode();
            var hashCode2 = scanner2.GetHashCode();

            Assert.AreNotEqual(hashCode1, hashCode2);
        }

        [Test]
        public void Should_not_be_possible_not_to_define_an_assembly()
        {
            Assert.Throws<ArgumentNullException>(() => new AssemblyTypesScanner(null));
        }
    }
}