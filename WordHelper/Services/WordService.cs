using WordHelper.DTOs;
using Microsoft.Extensions.Logging;
using WordHelper.Integrations;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text.Json;
using System.Text;

namespace WordHelper.Services
{
    public class WordService : IWordService
    {
        private readonly ILogger<WordService> _logger;
        private readonly OpenRouterClient _openRouterClient;
        private readonly AnkiClient _ankiService;

        public WordService(ILogger<WordService> logger,  OpenRouterClient openRouterClient, AnkiClient ankiService)
        {
            _logger = logger;
            _openRouterClient = openRouterClient;
            _ankiService = ankiService;
        }

        public async Task<WordResponseDto> GetWordInfoAsync(string word)
        {

            _logger.LogInformation("Getting word info from Dictionary API for: {Word}", word);

            var response = new WordResponseDto();


                var (definitionText, exampleSentences) = await _openRouterClient.GetDefinitionAndExamplesAsync(word);
                response.Definition = definitionText;
                response.ExampleSentences = exampleSentences;


                var success = await _ankiService.AddVocabularyNoteAsync(response, word);
                if (success)
                {
                    _logger.LogInformation("已成功將單字「{Word}」加入 Anki。", word);
                }
                else
                {
                    _logger.LogWarning("單字「{Word}」未成功加入 Anki。", word);
                }


            return response;

        }     
    }
}
