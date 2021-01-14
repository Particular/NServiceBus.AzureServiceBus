namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Creation
{
    using System;
    using System.IO;
    using System.Text;
    using Logging;
    using NUnit.Framework;
    using Settings;
    using Testing;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_creating_the_transport
    {
        StringBuilder logStatements;
        IDisposable scope;

        [SetUp]
        public void SetUp()
        {
            logStatements = new StringBuilder();

            scope = LogManager.Use<TestingLoggerFactory>()
                .BeginScope(new StringWriter(logStatements), LogLevel.Warn);
        }

        [TearDown]
        public void Teardown() => scope.Dispose();

        [Test]
        public void Should_log_a_deprecation_warning()
        {
            var transport = new AzureServiceBusTransport();

            try
            {
                transport.Initialize(new SettingsHolder(), "connectionString");
            }
            catch
            {
                // ignored
            }

            StringAssert.Contains(AzureServiceBusTransport.DeprecationMessage, logStatements.ToString());
        }
    }
}