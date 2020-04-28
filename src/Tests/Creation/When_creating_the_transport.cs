namespace NServiceBus.Azure.WindowsAzureServiceBus.Tests.Creation
{
    using System;
    using Logging;
    using NUnit.Framework;
    using Settings;

    [TestFixture]
    [Category("AzureServiceBus")]
    public class When_creating_the_transport
    {
        [Test]
        public void Should_log_a_deprecation_warning()
        {
            var loggerDefinition = LogManager.Use<VerificationLoggerDefinition>();
            loggerDefinition.Level(LogLevel.Warn);

            var transport = new AzureServiceBusTransport();

            try
            {
                transport.Initialize(new SettingsHolder(), "connectionString");
            }
            catch
            {
                // ignored
            }

            Assert.AreEqual(AzureServiceBusTransport.DeprecationMessage, VerificationLogger.LastWarnMessage);
        }
    }

    class VerificationLoggerDefinition : LoggingFactoryDefinition
    {
        LogLevel level = LogLevel.Info;

        public void Level(LogLevel level) => this.level = level;
        protected override ILoggerFactory GetLoggingFactory() => new VerificationLoggerFactory(level);
    }

    class VerificationLoggerFactory : ILoggerFactory
    {
        LogLevel level;

        public VerificationLoggerFactory(LogLevel level) => this.level = level;
        public ILog GetLogger(Type type) => GetLogger(type.FullName);
        public ILog GetLogger(string name) => new VerificationLogger(name, level);
    }

    class VerificationLogger : ILog
    {
        public VerificationLogger(string name, LogLevel level)
        {
            IsDebugEnabled = LogLevel.Debug >= level;
            IsInfoEnabled = LogLevel.Info >= level;
            IsWarnEnabled = LogLevel.Warn >= level;
            IsErrorEnabled = LogLevel.Error >= level;
            IsFatalEnabled = LogLevel.Fatal >= level;
        }

        public void Warn(string message) => LastWarnMessage = message;

        public void Warn(string message, Exception exception) => throw new NotImplementedException();
        public void WarnFormat(string format, params object[] args) => throw new NotImplementedException();
        public void Debug(string message) => throw new NotImplementedException();
        public void Debug(string message, Exception exception) => throw new NotImplementedException();
        public void DebugFormat(string format, params object[] args) => throw new NotImplementedException();
        public void Info(string message) => throw new NotImplementedException();
        public void Info(string message, Exception exception) => throw new NotImplementedException();
        public void InfoFormat(string format, params object[] args) => throw new NotImplementedException();
        public void Error(string message) => throw new NotImplementedException();
        public void Error(string message, Exception exception) => throw new NotImplementedException();
        public void ErrorFormat(string format, params object[] args) => throw new NotImplementedException();
        public void Fatal(string message) => throw new NotImplementedException();
        public void Fatal(string message, Exception exception) => throw new NotImplementedException();
        public void FatalFormat(string format, params object[] args) => throw new NotImplementedException();

        public bool IsDebugEnabled { get; }
        public bool IsInfoEnabled { get; }
        public bool IsWarnEnabled { get; }
        public bool IsErrorEnabled { get; }
        public bool IsFatalEnabled { get; }
        public static string LastWarnMessage;
    }
}