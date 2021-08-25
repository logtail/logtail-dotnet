using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Logtail
{
    public sealed class Log
    {
        [JsonProperty("dt")]
        public DateTimeOffset Timestamp { get; set; }

        public string Message { get; set; }

        public string Level { get; set; }

        public Dictionary<string, object> Context { get; set; }

        public Log() {
            Timestamp = DateTimeOffset.UtcNow;
        }
    }
}
