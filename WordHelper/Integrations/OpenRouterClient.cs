using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using WordHelper.DTOs;
using WordHelper.Options;
namespace WordHelper.Integrations
{
    public class OpenRouterClient
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly OpenRouterOptions _options;

        public OpenRouterClient(HttpClient httpClient, IConfiguration configuration, IOptions<OpenRouterOptions> options)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _options = options.Value;
        }

        public async Task<(string Definition, List<string> Examples)> GetDefinitionAndExamplesAsync(string word)
        {
            var apiKey = _options.ApiKey;
            if (string.IsNullOrEmpty(apiKey)) throw new Exception("Missing OpenRouter API Key");

            var prompt = $$"""
                You are a dictionary assistant.
                Please provide the definition and 3 example sentences for the English word: "{{word}}".
                
                Respond only in the following JSON format:
                
                {
                        
                          "definition": "A brief definition of the word.",
                  "exampleSentences": [
                    "Example sentence 1.",
                    "Example sentence 2.",
                    "Example sentence 3."
                  ]
                }
                
                Only return valid JSON. Do not include explanations or extra text.
                """;

            var requestBody = new
            {
                model = "mistralai/mistral-7b-instruct",
                messages = new[]
                {
            new { role = "system", content = "You are an English dictionary assistant." },
            new { role = "user", content = prompt }
        }
            };

            var requestJson = JsonSerializer.Serialize(requestBody);
            var request = new HttpRequestMessage(HttpMethod.Post, "https://openrouter.ai/api/v1/chat/completions");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
            request.Headers.Add("HTTP-Referer", "https://your-app.com");
            request.Headers.Add("X-Title", "WordHelperApp");
            request.Content = new StringContent(requestJson, Encoding.UTF8, "application/json");

            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"OpenRouter Error: {response.StatusCode} - {error}");
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseJson);
            var content = doc.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            // 解析回傳的 JSON 結構
            var obj = JsonSerializer.Deserialize<JsonElement>(content!);

            var def = obj.GetProperty("definition").GetString() ?? "";
            var examples = obj.GetProperty("exampleSentences").EnumerateArray().Select(e => e.GetString() ?? "").ToList();

            return (def, examples);
        }

    }
}
