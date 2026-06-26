using Artifex.MyClass;
using Artifex.Request;
using Artifex.Response;

namespace Artifex.Services
{
    public class LLMService
    {
        private readonly HttpClient _client;

        public LLMService(HttpClient client)
        {
            _client = client;
        }

        public async Task<LLMChatResponse> ChatAsync(string prompt)
        {
            var request = new LLMChatRequest
            {
                Model = "gemma4:latest",
                Stream = false,
                Messages =
                [
                    new LLMMessage
                {
                    Role = "user",
                    Content = prompt
                }
                ]
            };

            var response = await _client.PostAsJsonAsync("/api/chat", request);
            // var body = await response.Content.ReadAsStringAsync();
            // Console.WriteLine(body);
            response.EnsureSuccessStatusCode();

            var result =
                await response.Content.ReadFromJsonAsync<LLMChatResponse>();

            return result!;
        }
    }
}

