namespace NServiceBus.AzureServiceBus.Topology.MetaModel
{
    using System;
    using System.Text.RegularExpressions;

    class ConnectionString : IEquatable<ConnectionString>
    {
        public static readonly string Sample = "Endpoint=sb://[namespace name].servicebus.windows.net;SharedAccessKeyName=[shared access key name];SharedAccessKey=[shared access key]";
        private static readonly string Pattern = "^Endpoint=sb://(?<namespaceName>[A-Za-z][A-Za-z0-9-]{4,48}[A-Za-z0-9]).servicebus.windows.net;SharedAccessKeyName=(?<sharedAccessPolicyName>[\\w\\W]+);SharedAccessKey=(?<sharedAccessPolicyValue>[\\w\\W]+)$";

        private readonly string _value;

        public string NamespaceName { get; }
        public string SharedAccessPolicyName { get; }
        public string SharedAccessPolicyValue { get; }

        public ConnectionString(string value)
        {
            if (!Regex.IsMatch(value, Pattern, RegexOptions.IgnoreCase))
                throw new ArgumentException($"Provided value isn't a valid connection string. {Environment.NewLine}" +
                                            $"The namespace name can contain only letters, numbers, and hyphens.The namespace must start with a letter, and it must end with a letter or number. {Environment.NewLine}" +
                                            $"f.e.: {ConnectionString.Sample}", nameof(value));

            _value = value;

            NamespaceName = Regex.Match(value, Pattern).Groups["namespaceName"].Value;
            SharedAccessPolicyName = Regex.Match(value, Pattern).Groups["sharedAccessPolicyName"].Value;
            SharedAccessPolicyValue = Regex.Match(value, Pattern).Groups["sharedAccessPolicyValue"].Value;
        }

        public bool Equals(ConnectionString other)
        {
            return other != null
                && string.Equals(NamespaceName, other.NamespaceName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(SharedAccessPolicyName, other.SharedAccessPolicyName, StringComparison.OrdinalIgnoreCase)
                && string.Equals(SharedAccessPolicyValue, other.SharedAccessPolicyValue);
        }

        public override bool Equals(object obj)
        {
            var target = obj as ConnectionString;
            return Equals(target);
        }

        public override int GetHashCode()
        {
            var namespaceName = NamespaceName.ToLower();
            var sharedAccessPolicyName = SharedAccessPolicyName.ToLower();

            return string.Concat(namespaceName, sharedAccessPolicyName, SharedAccessPolicyValue).GetHashCode();
        }

        public static bool TryParse(string value, out ConnectionString connectionString)
        {
            try
            {
                connectionString = new ConnectionString(value);
                return true;
            }
            catch (ArgumentException)
            {
                connectionString = null;
                return false;
            }
        }

        public static implicit operator string(ConnectionString connectionString)
        {
            return connectionString._value;
        }
    }
}
