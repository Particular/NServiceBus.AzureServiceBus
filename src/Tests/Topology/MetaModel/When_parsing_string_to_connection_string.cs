namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Topology.MetaModel
{
    using Transport.AzureServiceBus;
    using NUnit.Framework;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_parsing_string_to_connection_string
    {
        static string Template = "Endpoint=sb://{0}.servicebus.windows.net;SharedAccessKeyName={1};SharedAccessKey={2}";

        [Test]
        [TestCase("a")]
        [TestCase("abcde")]
        public void Should_return_false_if_input_length_is_shorter_than_6_chars(string value)
        {
            var @namespace = string.Format(Template, value, "RootManageSharedAccessKey", "YourSecret");

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
            var @namespace = string.Format(Template, value, "RootManageSharedAccessKey", "YourSecret");

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
            var @namespace = string.Format(Template, value, "RootManageSharedAccessKey", "YourSecret");

            ConnectionString connectionString;
            var isValid = ConnectionString.TryParse(@namespace, out connectionString);

            Assert.IsFalse(isValid);
            Assert.Null(connectionString);
        }

        [Test]
        public void Should_return_false_if_input_does_not_finish_with_a_letter_or_a_number()
        {
            var @namespace = string.Format(Template, "abcdef-", "RootManageSharedAccessKey", "YourSecret");

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
            var @namespace = string.Format(Template, value, "RootManageSharedAccessKey", "YourSecret");

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
            var @namespace = string.Format(Template, value, "RootManageSharedAccessKey", "YourSecret");

            ConnectionString connectionString;
            var isValid = ConnectionString.TryParse(@namespace, out connectionString);

            Assert.True(isValid);
            Assert.NotNull(connectionString);
        }

        [Test]
        public void Should_extract_namespace_name_and_shared_access_policy_name_and_shared_access_policy_value()
        {
            var @namespace = string.Format(Template, "namespace", "RootManageSharedAccessKey", "YourSecret");

            var connectionString = new ConnectionString(@namespace);

            Assert.AreEqual("namespace", connectionString.NamespaceName);
            Assert.AreEqual("RootManageSharedAccessKey", connectionString.SharedAccessPolicyName);
            Assert.AreEqual("YourSecret", connectionString.SharedAccessPolicyValue);
        }

        [Test]
        public void Two_connection_strings_are_different_if_namespace_names_are_different()
        {
            var namespace1 = string.Format(Template, "namespace1", "RootManageSharedAccessKey", "YourSecret");
            var namespace2 = string.Format(Template, "namespace2", "RootManageSharedAccessKey", "YourSecret");
            
            var connectionString1 = new ConnectionString(namespace1);
            var connectionString2 = new ConnectionString(namespace2);

            Assert.AreNotEqual(connectionString1, connectionString2);
        }

        [Test]
        public void Two_connection_strings_are_different_if_shared_access_policy_names_are_different()
        {
            var namespace1 = string.Format(Template, "namespace", "RootManageSharedAccessKey1", "YourSecret");
            var namespace2 = string.Format(Template, "namespace", "RootManageSharedAccessKey2", "YourSecret");

            var connectionString1 = new ConnectionString(namespace1);
            var connectionString2 = new ConnectionString(namespace2);

            Assert.AreNotEqual(connectionString1, connectionString2);
        }

        [Test]
        [TestCase("YourSecret", "yoursecret")]
        [TestCase("YourSecret", "YOURSECRET")]
        [TestCase("YourSecret1", "YourSecret2")]
        public void Two_connection_strings_are_different_if_shared_access_policy_values_are_differentwith_case_sensitive_check(string value1, string value2)
        {
            var namespace1 = string.Format(Template, "namespace", "RootManageSharedAccessKey", value1);
            var namespace2 = string.Format(Template, "namespace", "RootManageSharedAccessKey", value2);

            var connectionString1 = new ConnectionString(namespace1);
            var connectionString2 = new ConnectionString(namespace2);

            Assert.AreNotEqual(connectionString1, connectionString2);
        }

        [Test]
        [TestCase("namespace", "namespace")]
        [TestCase("namespace", "Namespace")]
        [TestCase("namespace", "NAMESPACE")]
        public void Two_connection_strings_are_equal_if_namespace_names_are_equal_with_case_insensitive_check_and_other_components_are_the_same(string value1, string value2)
        {
            var namespace1 = string.Format(Template, value1, "RootManageSharedAccessKey", "YourSecret");
            var namespace2 = string.Format(Template, value2, "RootManageSharedAccessKey", "YourSecret");

            var connectionString1 = new ConnectionString(namespace1);
            var connectionString2 = new ConnectionString(namespace2);

            Assert.AreEqual(connectionString1, connectionString2);
        }

        [Test]
        [TestCase("RootManageSharedAccessKey", "RootManageSharedAccessKey")]
        [TestCase("RootManageSharedAccessKey", "rootmanagesharedaccesskey")]
        [TestCase("RootManageSharedAccessKey", "ROOTMANAGESHAREDACCESSKEY")]
        public void Two_connection_strings_are_equal_if_shared_access_policy_names_are_equal_with_case_insensitive_check_and_other_components_are_the_same(string value1, string value2)
        {
            var namespace1 = string.Format(Template, "namespace", value1, "YourSecret");
            var namespace2 = string.Format(Template, "namespace", value2, "YourSecret");

            var connectionString1 = new ConnectionString(namespace1);
            var connectionString2 = new ConnectionString(namespace2);

            Assert.AreEqual(connectionString1, connectionString2);
        }

        [Test]
        public void Two_connection_strings_are_equal_if_shared_access_policy_values_are_equal_with_case_sensitive_check_and_other_components_are_the_same()
        {
            var namespace1 = string.Format(Template, "namespace", "RootManageSharedAccessKey", "YourSecret");
            var namespace2 = string.Format(Template, "namespace", "RootManageSharedAccessKey", "YourSecret");

            var connectionString1 = new ConnectionString(namespace1);
            var connectionString2 = new ConnectionString(namespace2);

            Assert.AreEqual(connectionString1, connectionString2);
        }

        [Test]
        public void Two_connection_strings_are_different_if_one_of_them_is_null()
        {
            var @namespace = string.Format(Template, "namespace", "RootManageSharedAccessKey", "YourSecret");

            var connectionString = new ConnectionString(@namespace);
            var areEqual = connectionString.Equals(null);

            Assert.False(areEqual);
        }

        [Test]
        [TestCase("namespace", "namespace", "RootManageSharedAccessKey", "RootManageSharedAccessKey")]
        [TestCase("namespace", "Namespace", "RootManageSharedAccessKey", "rootmanagesharedaccesskey")]
        [TestCase("namespace", "NAMESPACE", "RootManageSharedAccessKey", "ROOTMANAGESHAREDACCESSKEY")]
        public void Two_connection_strings_have_the_same_hash_code_if_they_are_equal(string namespaceName1, string namespaceName2, string sharedAccessPolicyName1, string sharedAccessPolicyName2)
        {
            var namespace1 = string.Format(Template, namespaceName1, sharedAccessPolicyName1, "YourSecret");
            var namespace2 = string.Format(Template, namespaceName2, sharedAccessPolicyName2, "YourSecret");

            var connectionString1 = new ConnectionString(namespace1);
            var connectionString2 = new ConnectionString(namespace2);

            var hashCode1 = connectionString1.GetHashCode();
            var hashCode2 = connectionString2.GetHashCode();

            Assert.AreEqual(hashCode1, hashCode2);
        }
    }
}
