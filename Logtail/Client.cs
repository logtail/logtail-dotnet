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
            try
            {
                var response = await httpClient.PostAsync("/", content);
                return response.IsSuccessStatusCode;
            }
            catch (TaskCanceledException) { } // request timed out
            catch (HttpRequestException) { } // some networking error

            return false;
        }

        private HttpContent serialize(IEnumerable<Log> logs) {
            var payload = JsonConvert.SerializeObject(logs, settings);
            return new StringContent(payload, Encoding.UTF8, "application/json");
        }
    }
}
