using System;
using System.Collections.Generic;
using NLog;
using NLog.Config;
using NLog.Targets;
using NLog.Layouts;

namespace Logtail.NLog
{
    /// <summary>
    /// NLog target for Logtail. This target does not send all the events individually
    /// to the Logtail server but it sends them periodically in batches.
    /// </summary>
    [Target("Logtail")]
    public sealed class LogtailTarget : TargetWithContext
    {
        /// <summary>
        /// Gets or sets the Logtail source token.
        /// </summary>
        /// <value>The source token.</value>
        [RequiredParameter]
        public Layout SourceToken { get; set; }

        /// <summary>
        /// The Logtail endpoint.
        /// </summary>
        public Layout Endpoint { get; set; } = "https://in.logtail.com";

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

        /// <summary>
        /// We capture the file and line of every log message by default. You can turn this
        /// option off if it has negative impact on the performance of your application.
        /// </summary>
        public bool CaptureSourceLocation
        {
            get => StackTraceUsage == StackTraceUsage.Max;
            set => StackTraceUsage = value ? StackTraceUsage.Max : StackTraceUsage.None;
        }

        /// <summary>
        /// Include GlobalDiagnosticContext in logs.
        /// </summary>
        public bool IncludeGlobalDiagnosticContext { get; set; } = true;

        /// <summary>
        /// Control callsite capture of source-file and source-linenumber.
        /// </summary>
        public StackTraceUsage StackTraceUsage
        {
            get => _stackTraceUsage;
            set
            {
                if (value == StackTraceUsage.None)
                {
                    IncludeCallSite = false;
                    IncludeCallSiteStackTrace = false;
                }
                else
                {
                    IncludeCallSite = true;
                    IncludeCallSiteStackTrace = value == StackTraceUsage.Max;
                }
                _stackTraceUsage = value;
            }
        }
        private StackTraceUsage _stackTraceUsage;

        private Drain logtail = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="LogtailTarget"/> class.
        /// </summary>
        public LogtailTarget()
        {
            StackTraceUsage = StackTraceUsage.Max;
        }

        /// <inheritdoc/>
        protected override void InitializeTarget()
        {
            logtail?.Stop().Wait();

            var sourceToken = RenderLogEvent(SourceToken, LogEventInfo.CreateNullEvent());
            var endpoint = RenderLogEvent(Endpoint, LogEventInfo.CreateNullEvent());

            var client = new Client(
                sourceToken,
                endpoint: endpoint,
                retries: Retries
            );

            logtail = new Drain(
                client,
                period: TimeSpan.FromMilliseconds(FlushPeriodMilliseconds),
                maxBatchSize: MaxBatchSize
            );

            base.InitializeTarget();
        }

        /// <inheritdoc/>
        protected override void CloseTarget()
        {
            logtail?.Stop().Wait();
            base.CloseTarget();
        }

        /// <inheritdoc/>
        protected override void Write(LogEventInfo logEvent)
        {
            var contextDictionary = new Dictionary<string, object> {
                ["logger"] = logEvent.LoggerName,
                ["properties"] = logEvent.Properties,
                ["runtime"] = new Dictionary<string, object> {
                    ["class"] = logEvent.CallerClassName,
                    ["member"] = logEvent.CallerMemberName,
                    ["file"] = string.IsNullOrEmpty(logEvent.CallerFilePath) ? null : logEvent.CallerFilePath,
                    ["line"] = string.IsNullOrEmpty(logEvent.CallerFilePath) ? null : logEvent.CallerLineNumber as int?,
                },
            };

            if (IncludeGlobalDiagnosticContext) {
                var gdcKeys = GlobalDiagnosticsContext.GetNames();

                if (gdcKeys.Count > 0) {
                    var gdcDict = new Dictionary<string, object>();

                    foreach (string key in gdcKeys) {
                        if (string.IsNullOrEmpty(key)) continue;
                        gdcDict[key] = GlobalDiagnosticsContext.GetObject(key);
                    }

                    contextDictionary["gdc"] = gdcDict;
                }
            }
            string logMessage = RenderLogEvent(this.Layout, logEvent);

            var log = new Log {
                Timestamp = new DateTimeOffset(logEvent.TimeStamp),
                Message = logMessage,
                Level = logEvent.Level.Name,
                Context = contextDictionary
            };

            logtail.Enqueue(log);
        }
    }
}
