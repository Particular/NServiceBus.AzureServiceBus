namespace NServiceBus.AzureServiceBus.EnduranceTests.Targets
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Net;
    using NLog;
    using NLog.Targets;
    using Newtonsoft.Json;
    using NLog.Config;
    using NLog.Layouts;
    using NServiceBus.AzureServiceBus.EnduranceTests.TestUtils;

    [Target("Slack")]
    public sealed class SlackTarget : TargetWithLayout
    {
        private Uri _uri = null;

        [RequiredParameter]
        public string WebhookUri { get { return _uri?.ToString() ?? string.Empty; } set { _uri = new Uri(value);} }

        public string StackTraceLayout { get; set; } = "${stacktrace}";

        protected override void Write(LogEventInfo logEvent)
        {
            var logMessage = Layout.Render(logEvent);

            SendMessageToSlack(new SlackMessage
            {
                attachments = new List<SlackAttachment>
                {
                    new SlackAttachment
                    {
                        title = logEvent.LoggerName,
                        title_link = BlobHelper.BuildBlobUrlFromLogEvent(TestEnvironment.AzureStorage, logEvent),
                        text = logMessage,
                        fallback = logMessage,
                        color = "danger"
                    },
                    new SlackAttachment
                    {
                        text = (new SimpleLayout(StackTraceLayout)).Render(logEvent)
                    }
                }
            });
        }

        private void SendMessageToSlack(SlackMessage message)
        {
            using (var client = new WebClient())
            {
                var data = new NameValueCollection()
                {
                    {"payload", JsonConvert.SerializeObject(message)}
                };

                client.UploadValuesAsync(_uri, "POST", data);
            }
        }

        private class SlackMessage
        {
            public string text { get; set; }
            public List<SlackAttachment> attachments { get; set; }
        }

        private class SlackAttachment
        {
            public string fallback { get; set; }
            public string pretext { get; set; }
            public string color { get; set; }
            public string title { get; set; }
            public string title_link { get; set; }
            public string text { get; set; }
        }
    }
}
