namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology.EventsScanner
{
    using NServiceBus.AzureServiceBus.TypesScanner;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_configuring_an_assembly_scanner
    {
        [Test]
        [TestCase("assemblyName", "assemblyName")]
        [TestCase("AssemblyName", "assemblyname")]
        [TestCase("ASSEMBLYNAME", "assemblyname")]
        public void Two_scanners_should_be_equal_if_they_reference_the_same_assembly(string assemblyName1, string assemblyName2)
        {
            var scanner1 = new AssemblyTypesScanner(assemblyName1);
            var scanner2 = new AssemblyTypesScanner(assemblyName2);

            Assert.AreEqual(scanner1, scanner2);

            var hashCode1 = scanner1.GetHashCode();
            var hashCode2 = scanner2.GetHashCode();

            Assert.AreEqual(hashCode1, hashCode2);
        }

        [Test]
        public void Two_scanners_should_be_not_equal_if_they_reference_two_differents_assemblies()
        {
            var scanner1 = new AssemblyTypesScanner("assemblyName1");
            var scanner2 = new AssemblyTypesScanner("assemblyName2");

            Assert.AreNotEqual(scanner1, scanner2);

            var hashCode1 = scanner1.GetHashCode();
            var hashCode2 = scanner2.GetHashCode();

            Assert.AreNotEqual(hashCode1, hashCode2);
        }
    }
}