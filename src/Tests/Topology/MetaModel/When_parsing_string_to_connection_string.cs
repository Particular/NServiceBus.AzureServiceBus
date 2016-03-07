namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology.MetaModel
{
    using NServiceBus.AzureServiceBus.Topology.MetaModel;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_parsing_string_to_connection_string
    {
        private static readonly string Template = "Endpoint=sb://{0}.servicebus.windows.net;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=YourSecret";

        [Test]
        [TestCase("a")]
        [TestCase("abcde")]
        public void Should_return_false_if_input_length_is_shorter_than_6_chars(string value)
        {
            var @namespace = string.Format(Template, value);

            ConnectionString connectionString;
            var isValid = ConnectionString.TryParse(@namespace, out connectionString);

            Assert.IsFalse(isValid);
            Assert.Null(connectionString);
        }

        [Test]
        [TestCase("abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxy")]
        [TestCase("abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz")]
        public void Should_return_false_if_input_length_is_longer_than_50_chars(string value)
        {
            var @namespace = string.Format(Template, value);

            ConnectionString connectionString;
            var isValid = ConnectionString.TryParse(@namespace, out connectionString);

            Assert.IsFalse(isValid);
            Assert.Null(connectionString);
        }

        [Test]
        [TestCase("1abcdef")]
        [TestCase("-abcdef")]
        public void Should_return_false_if_input_does_not_start_with_a_letter(string value)
        {
            var @namespace = string.Format(Template, value);

            ConnectionString connectionString;
            var isValid = ConnectionString.TryParse(@namespace, out connectionString);

            Assert.IsFalse(isValid);
            Assert.Null(connectionString);
        }

        [Test]
        public void Should_return_false_if_input_does_not_finish_with_a_letter_or_a_number()
        {
            var @namespace = string.Format(Template, "abcdef-");

            ConnectionString connectionString;
            var isValid = ConnectionString.TryParse(@namespace, out connectionString);

            Assert.IsFalse(isValid);
            Assert.Null(connectionString);
        }

        [Test]
        [TestCase("abcdef@")]
        [TestCase("abcdef#")]
        [TestCase("abcdef$")]
        [TestCase("abcdef%")]
        [TestCase("abcdef^")]
        [TestCase("abcdef&")]
        public void Should_return_false_if_input_contains_not_valid_chars(string value)
        {
            var @namespace = string.Format(Template, value);

            ConnectionString connectionString;
            var isValid = ConnectionString.TryParse(@namespace, out connectionString);

            Assert.IsFalse(isValid);
            Assert.Null(connectionString);
        }

        [Test]
        [TestCase("abcdef")]
        [TestCase("ABCDEF")]
        [TestCase("abcdef1")]
        [TestCase("ABCDEF1")]
        [TestCase("abcd-ef")]
        [TestCase("ABCD-EF")]
        public void Should_return_true_if_input_follows_namespace_name_rules(string value)
        {
            var @namespace = string.Format(Template, value);

            ConnectionString connectionString;
            var isValid = ConnectionString.TryParse(@namespace, out connectionString);

            Assert.True(isValid);
            Assert.NotNull(connectionString);
        }

        [Test]
        public void Should_extract_namespace_name_and_shared_access_policy_name_and_shared_access_policy_value()
        {
            var @namespace = string.Format(Template, "namespace");

            var connectionString = new ConnectionString(@namespace);

            Assert.AreEqual("namespace", connectionString.NamespaceName);
            Assert.AreEqual("RootManageSharedAccessKey", connectionString.SharedAccessPolicyName);
            Assert.AreEqual("YourSecret", connectionString.SharedAccessPolicyValue);
        }
    }
}
