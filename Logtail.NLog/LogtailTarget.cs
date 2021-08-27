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
    [Target("Logtail")]
    public sealed class LogtailTarget : Target
    {
        /// <summary>
        /// Gets or sets the Logtail source token.
        /// </summary>
        /// <value>The source token.</value>
        [RequiredParameter]
        public string SourceToken { get; set; }

        /// <summary>
        /// The Logtail endpoint.
        /// </summary>
        public string Endpoint { get; set; } = "https://in.logtail.com";

        /// <summary>
        /// Maximum logs sent to the server in one batch.
        /// </summary>
        public int MaxBatchSize { get; set; } = 1000;

        /// <summary>
        /// The flushing period in milliseconds.
        /// </summary>
        public int FlushPeriodMilliseconds { get; set; } = 250;

        /// <summary>
        /// The number of retries of failing HTTP requests.
        /// </summary>
        public int Retries { get; set; } = 10;

        private Drain logtail = null;

        protected override void InitializeTarget()
        {
            logtail?.Stop().Wait();

            var client = new Client(
                SourceToken,
                endpoint: Endpoint,
                retries: Retries
            );

            logtail = new Drain(
                client,
                period: TimeSpan.FromMilliseconds(FlushPeriodMilliseconds),
                maxBatchSize: MaxBatchSize
            );

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
                        ["line"] = logEvent.CallerFilePath != null ? logEvent.CallerLineNumber as int? : null,
                    },
                }
            };

            logtail.Enqueue(log);
        }
    }
}
