namespace WordHelper.DTOs
{
    public class WordResponseDto
    {
        public string Definition { get; set; } = string.Empty;
        public List<string> ExampleSentences { get; set; } = new();

    }
}
