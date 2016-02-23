namespace NServiceBus.AzureServiceBus.EnduranceTests.LayoutRenderer
{
    using System;
    using System.Linq;
    using System.Text;
    using NLog;
    using NLog.Config;
    using NLog.LayoutRenderers;

    /// <summary>
    /// Exception information provided through
    /// a call to one of the Logger.*Exception() methods.
    /// </summary>
    [LayoutRenderer("aggregateexception")]
    [ThreadAgnostic]
    public class AggregateExceptionLayoutRenderer : ExceptionLayoutRenderer
    {
        protected override void Append(StringBuilder builder, LogEventInfo logEvent)
        {
            base.Append(builder, logEvent);

            var exception = logEvent.Exception as AggregateException;

            if (exception == null)
            {
                return;
            }

            builder.AppendLine(string.Empty);
            builder.AppendLine(string.Empty);
            builder.AppendLine("AggregateException Inner Exceptions:");

            exception.Flatten();

            foreach (var innerLogEvent in exception.InnerExceptions.Select(iex => new LogEventInfo
            {
                Exception = iex
            }))
            {
                builder.AppendLine(string.Empty);
                Append(builder, innerLogEvent);
            }
        }
    }
}
