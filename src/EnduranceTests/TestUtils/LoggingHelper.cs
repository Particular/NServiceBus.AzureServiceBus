namespace NServiceBus.AzureServiceBus.EnduranceTests.TestUtils
{
    using NLog;
    using NLog.Config;
    using NLog.Targets;
    using NLog.Targets.Wrappers;
    using NServiceBus.AzureServiceBus.EnduranceTests.Targets;
    using System;
    using System.Threading.Tasks;
    using NServiceBus.AzureServiceBus.EnduranceTests.LayoutRenderer;


    public static class LoggingHelper
    {
        internal static Logger Log = LogManager.GetLogger("Unhandled Logger");

        public static void Configure()
        {
            ConfigurationItemFactory.Default.Targets
                                        .RegisterDefinition("Slack", typeof(SlackTarget));

            ConfigurationItemFactory.Default.Targets.RegisterDefinition("AzureBlobErrorOnly", typeof(AzureBlobErrorOnlyTarget));

            ConfigurationItemFactory.Default.LayoutRenderers.RegisterDefinition("aggregateexception", typeof(AggregateExceptionLayoutRenderer));

            var logConfig = new LoggingConfiguration();

            var consoleTarget = new ColoredConsoleTarget
            {
                Layout = @"${level}|${logger}|${message}${onexception:${newline}${aggregateexception:format=type,message,method}}"
            };

            logConfig.AddTarget("console", consoleTarget);
            logConfig.LoggingRules.Add(new LoggingRule("*", LogLevel.Debug, consoleTarget));


            var bufferedTarget = new BufferingTargetWrapper
            {
                WrappedTarget = new SlackTarget
                {
                    Layout = @"${date:format=yyyy-MM-dd HH\:mm\:ss} ${message}",
                    WebhookUri = TestEnvironment.SlackWebhookUri
                },
                BufferSize = TestSettings.SlackBufferSize,
                FlushTimeout = TestSettings.SlackTimeoutInMilliseconds
            };

            logConfig.AddTarget("slack", bufferedTarget);
            logConfig.LoggingRules.Add(new LoggingRule("*", LogLevel.Error, bufferedTarget));

            var blobTarget = new AzureBlobErrorOnlyTarget
            {
                StorageAccount = TestEnvironment.AzureStorage,
                Layout = @"TimeStamp: ${date:format=yyyy-MM-dd HH\:mm\:ss}${newline}Logger: ${logger}${newline}Message: ${message}${newline}Exception:${newline}${aggregateexception:format=type,message,method,stacktrace:Separator=\r\n:InnerExceptionSeparator=\r\n:maxInnerExceptionLevel=5:innerFormat=shortType,message,method,stacktrace}"
            };

            logConfig.AddTarget("blob", blobTarget);
            logConfig.LoggingRules.Add(new LoggingRule("*", LogLevel.Error, blobTarget));

            Console.WriteLine("Batched Errors to Slack w/Exception Blobs Links Configured");

            LogManager.Configuration = logConfig;

            Logging.LogManager.Use<NLogFactory>();

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            TaskScheduler.UnobservedTaskException += TaskSchedulerOnUnobservedTaskException;
        }

        private static void TaskSchedulerOnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs args)
        {
            Log.Error(args.Exception, "TaskScheduler Unobserved Task Exception");
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            var ex = (Exception)args.ExceptionObject;

            Log.Error(ex, "AppDomain Unhandled Exception");
        }
    }
}
