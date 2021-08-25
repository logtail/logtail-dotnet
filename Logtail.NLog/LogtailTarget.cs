using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;

namespace Logtail.NLog
{
    /// <summary>
    /// NLog target for Logtail. This target does not send all the events individually
    /// to the Logtail server but it sends them periodically in batches.
    /// </summary>
    /// <seealso cref="Target" />
    [Target("Logtail")]
    public sealed class LogtailTarget : Target
    {
        /// <summary>
        /// Gets or sets the name of the application.
        /// </summary>
        /// <value>The name of the application.</value>
        public string ApplicationName { get; set; }

        /// <summary>
        /// Gets or sets the Logtail source token.
        /// </summary>
        /// <value>The source token.</value>
        [RequiredParameter]
        public string SourceToken { get; set; }

        public string Endpoint { get; set; } = "https://in.logtail.com";

        private Drain logtail = null;

        protected override void InitializeTarget()
        {
            logtail?.Stop().Wait();

            var client = new Client(SourceToken, Endpoint);
            logtail = new Drain(client);

            base.InitializeTarget();
        }

        protected override void CloseTarget()
        {
            logtail?.Stop().Wait();
            base.CloseTarget();
        }

        protected override void Write(LogEventInfo logEvent)
        {
            var log = new Log {
                Timestamp = new DateTimeOffset(logEvent.TimeStamp),
                Message = logEvent.FormattedMessage,
                Level = logEvent.Level.Name,
                Context = new Dictionary<string, object> {
                    ["logger"] = logEvent.LoggerName,
                    ["properties"] = logEvent.Properties,
                    ["runtime"] = new Dictionary<string, object> {
                        ["class"] = logEvent.CallerClassName,
                        ["member"] = logEvent.CallerMemberName,
                        ["file"] = logEvent.CallerFilePath,
                        ["line"] = logEvent.CallerLineNumber,
                    },
                }
            };

            logtail.Enqueue(log);
        }
    }
}
