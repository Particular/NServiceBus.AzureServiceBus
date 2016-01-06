namespace NServiceBus.AzureServiceBus.Tests
{
    using NServiceBus.AzureServiceBus.Addressing;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_building_SHA1_based_name
    {
        [Test]
        public void Should_generate_a_properly_delimetered_name()
        {
            var result = new SHA1DeterministicNameBuilder().Build("TestEvent");
            Assert.AreEqual('-', result[5]);
            Assert.AreEqual('-', result[11]);
            Assert.AreEqual('-', result[17]);
            Assert.AreEqual('-', result[23]);
            Assert.AreEqual('-', result[29]);
            Assert.AreEqual('-', result[35]);
            Assert.AreEqual('-', result[41]);
        }

        [Test]
        public void Should_generate_a_formatted_unique_name_shorter_than_50_characters()
        {
            var result = new SHA1DeterministicNameBuilder().Build("TestEvent");
            Assert.AreEqual("58a82-ca4cd-b455f-b5fff-937e6-cbcbf-d794d-aafe2", result);
            Assert.AreEqual(47, result.Length);
        }
    }
}