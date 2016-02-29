namespace NServiceBus.AzureServiceBus.Topology.MetaModel
{
    using System;
    using System.Text.RegularExpressions;

    class ConnectionString
    {
        public static readonly string Sample = "Endpoint=sb://[namespace name].servicebus.windows.net;SharedAccessKeyName=[shared access key name];SharedAccessKey=[shared access key]";
        private static readonly string Pattern = "^Endpoint=sb://([A-Za-z][A-Za-z0-9-]{4,48}[A-Za-z0-9]).servicebus.windows.net;SharedAccessKeyName=([\\w\\W]+);SharedAccessKey=([\\w\\W]+)$";

        private readonly string _value;

        public ConnectionString(string value)
        {
            if (!Regex.IsMatch(value, Pattern, RegexOptions.IgnoreCase))
                throw new ArgumentException($"Provided value isn't a valid connection string. {Environment.NewLine}" +
                                            $"The namespace name can contain only letters, numbers, and hyphens.The namespace must start with a letter, and it must end with a letter or number. {Environment.NewLine}" +
                                            $"f.e.: {ConnectionString.Sample}", nameof(value));

            _value = value;
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
