﻿namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests
{
    class ConnectionStringValue
    {
        static string Template = "Endpoint=sb://{0}.servicebus.windows.net;SharedAccessKeyName={1};SharedAccessKey={2}";

        internal static readonly string Sample = Build();

        internal static string Build(string namespaceName = "namespace", string sharedAccessPolicyName = "RootManageSharedAccessKey", string sharedAccessPolicyValue = "YourSecret") => string.Format(Template, namespaceName, sharedAccessPolicyName, sharedAccessPolicyValue);
    }
}