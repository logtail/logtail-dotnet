using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Logtail
{
    /// <summary>
    /// The Client class is responsible for reliable delivery of logs
    /// to the Logtail servers.
    /// </summary>
    public sealed class Client
    {
        private readonly HttpClient httpClient;
        private readonly JsonSerializerSettings settings = new JsonSerializerSettings {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            ContractResolver = new DefaultContractResolver {
                NamingStrategy = new CamelCaseNamingStrategy()
            }
        };
        private readonly int retries;

        public Client(
            string sourceToken,
            string endpoint = "https://in.logtail.com",
            TimeSpan? timeout = null,
            int retries = 10
        )
        {
            settings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());
            settings.Converters.Add(new ToStringJsonConverter(typeof(System.Reflection.MemberInfo)));
            settings.Converters.Add(new ToStringJsonConverter(typeof(System.Reflection.Assembly)));
            settings.Converters.Add(new ToStringJsonConverter(typeof(System.Reflection.Module)));
            settings.Error = (sender, args) =>
            {
                args.ErrorContext.Handled = true;   // Ignore Properties that throws Exceptions
            };

            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {sourceToken}");
            httpClient.BaseAddress = new Uri(endpoint);
            httpClient.Timeout = timeout ?? TimeSpan.FromSeconds(10);

            this.retries = retries;
        }

        /// <summary>
        /// Sends a collection of logs to the server with several retries
        /// if an error occures.
        /// </summary>
        public async Task Send(IEnumerable<Log> logs)
        {
            var content = serialize(logs);

            for (int i = 0; i < retries; ++i) {
                await Task.Delay(TimeSpan.FromSeconds(i));

                var success = await sendOnce(content);
                if (success) break;
            }
        }

        private async Task<bool> sendOnce(HttpContent content)
        {
            try {
                var response = await httpClient.PostAsync("/", content);
                return response.IsSuccessStatusCode;
            } catch (TaskCanceledException) {
                // request timed out, silent error
            } catch (HttpRequestException) {
                // TODO: repeat only for certain HTTP errors (429, 5xx)
                // some networking error, silent error
            }

            return false;
        }

        private HttpContent serialize(IEnumerable<Log> logs) {
            var payload = JsonConvert.SerializeObject(logs, settings);
            var content = new ByteArrayContent(Encoding.UTF8.GetBytes(payload));
            content.Headers.Add("Content-Type", "application/json");
            return content;
        }

        /// <summary>
        /// JSON converter that just calls ToString on the target value (when non-null).
        /// This is configured as the converter for types that will otherwise spew a lot of irrelevant JSON
        /// into logs.
        /// </summary>
        internal sealed class ToStringJsonConverter : JsonConverter
        {
            private readonly System.Type _type;

            /// <inheritdoc />
            public override bool CanRead => false;

            public ToStringJsonConverter(System.Type type) =>
                _type = type;

            /// <inheritdoc />
            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                if (value is null)
                {
                    writer.WriteNull();
                }
                else
                {
                    writer.WriteValue(value.ToString());
                }
            }

            /// <inheritdoc />
            public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer) =>
                throw new NotSupportedException("Only serialization is supported");

            /// <inheritdoc />
            public override bool CanConvert(System.Type objectType) =>
                _type.IsAssignableFrom(objectType);
        }
    }
}
