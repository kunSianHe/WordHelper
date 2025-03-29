using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WordHelper.DTOs;
namespace WordHelper.Integrations
{
    public class AnkiClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AnkiClient> _logger;

        public AnkiClient(HttpClient httpClient, ILogger<AnkiClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<bool> AddVocabularyNoteAsync(WordResponseDto wordData, string word)
        {
            var payload = new
            {
                action = "addNote",
                version = 6,
                @params = new
                {
                    note = new
                    {
                        deckName = "WordHelper",
                        modelName = "Vocabulary",
                        fields = new
                        {
                            Word = word,
                            //Chinese = wordData.ChineseMeaning,
                            wordData.Definition,
                            Examples = string.Join("\n", wordData.ExampleSentences ?? new List<string>())
                        },
                        options = new
                        {
                            allowDuplicate = false
                        },
                        tags = new[] { "from_api" }
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync("http://localhost:8765", content);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Anki API 回傳失敗: {Status}", response.StatusCode);
                    return false;
                }

                var resultJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<AnkiResponse>(resultJson);

                return result?.Error == null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "傳送到 AnkiConnect 時發生錯誤");
                return false;
            }
        }

        private class AnkiResponse
        {
            public object Result { get; set; }
            public string Error { get; set; }
        }
    }
}
