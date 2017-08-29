// ASB doesn't work with the ScenarioContext.Current property which uses the logical CallContext
// as the OnMessage callback looses the previous CallContext.
// To avoid exceptions when accessing the logger, a custom logger with a fixed ScenarioContext needs to be used.
// This requires all tests to run sequentially as the logger is configured statically (LogManager).
namespace NServiceBus.AcceptanceTests
{
    using System;
    using System.Diagnostics;
    using AcceptanceTesting;
    using Logging;
    using NUnit.Framework;

    public partial class NServiceBusAcceptanceTest
    {
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            Scenario.GetLoggerFactory = ctx => new StaticLoggerFactory(ctx);
        }
    }

    class StaticLoggerFactory : ILoggerFactory
    {
        ScenarioContext currentContext;

        public StaticLoggerFactory(ScenarioContext currentContext)
        {
            this.currentContext = currentContext;
        }

        public ILog GetLogger(Type type)
        {
            return GetLogger(type.FullName);
        }

        public ILog GetLogger(string name)
        {
            return new StaticContextAppender(currentContext);
        }
    }

    class StaticContextAppender : ILog
    {
        ScenarioContext currentContext;

        public StaticContextAppender(ScenarioContext currentContext)
        {
            this.currentContext = currentContext;
        }

        public bool IsDebugEnabled => currentContext.LogLevel <= LogLevel.Debug;
        public bool IsInfoEnabled => currentContext.LogLevel <= LogLevel.Info;
        public bool IsWarnEnabled => currentContext.LogLevel <= LogLevel.Warn;
        public bool IsErrorEnabled => currentContext.LogLevel <= LogLevel.Error;
        public bool IsFatalEnabled => currentContext.LogLevel <= LogLevel.Fatal;


        public void Debug(string message)
        {
            Log(message, LogLevel.Debug);
        }

        public void Debug(string message, Exception exception)
        {
            var fullMessage = $"{message} {exception}";
            Log(fullMessage, LogLevel.Debug);
        }

        public void DebugFormat(string format, params object[] args)
        {
            var fullMessage = string.Format(format, args);
            Log(fullMessage, LogLevel.Debug);
        }

        public void Info(string message)
        {
            Log(message, LogLevel.Info);
        }


        public void Info(string message, Exception exception)
        {
            var fullMessage = $"{message} {exception}";
            Log(fullMessage, LogLevel.Info);
        }

        public void InfoFormat(string format, params object[] args)
        {
            var fullMessage = string.Format(format, args);
            Log(fullMessage, LogLevel.Info);
        }

        public void Warn(string message)
        {
            Log(message, LogLevel.Warn);
        }

        public void Warn(string message, Exception exception)
        {
            var fullMessage = $"{message} {exception}";
            Log(fullMessage, LogLevel.Warn);
        }

        public void WarnFormat(string format, params object[] args)
        {
            var fullMessage = string.Format(format, args);
            Log(fullMessage, LogLevel.Warn);
        }

        public void Error(string message)
        {
            Log(message, LogLevel.Error);
        }

        public void Error(string message, Exception exception)
        {
            var fullMessage = $"{message} {exception}";
            Log(fullMessage, LogLevel.Error);
        }

        public void ErrorFormat(string format, params object[] args)
        {
            var fullMessage = string.Format(format, args);
            Log(fullMessage, LogLevel.Error);
        }

        public void Fatal(string message)
        {
            Log(message, LogLevel.Fatal);
        }

        public void Fatal(string message, Exception exception)
        {
            var fullMessage = $"{message} {exception}";
            Log(fullMessage, LogLevel.Fatal);
        }

        public void FatalFormat(string format, params object[] args)
        {
            var fullMessage = string.Format(format, args);
            Log(fullMessage, LogLevel.Fatal);
        }

        void Log(string message, LogLevel messageSeverity)
        {
            if (currentContext.LogLevel > messageSeverity)
            {
                return;
            }

            Trace.WriteLine(message);
            currentContext.Logs.Enqueue(new ScenarioContext.LogItem
            {
                Level = messageSeverity,
                Message = message
            });
        }
    }
}