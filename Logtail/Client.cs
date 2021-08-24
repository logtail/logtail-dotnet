using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Logtail
{
    public class Client
    {
        private readonly HttpClient httpClient;
        private readonly int retries;

        private readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

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

        public async Task Send(IEnumerable<Log> logs)
        {
            var content = encodeJSON(logs);

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

        private HttpContent encodeJSON(IEnumerable<Log> logs) {
            var payload = JsonSerializer.SerializeToUtf8Bytes(logs, jsonOptions);
            var content = new ByteArrayContent(payload);
            content.Headers.Add("Content-Type", "application/json");
            return content;
        }
    }
}
