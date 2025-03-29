namespace WordHelper.DTOs
{
    public class MultipleChoiceDto
    {
        public string ClozeSentence { get; set; }
        public List<string> Options { get; set; }
        public string Answer { get; set; }
    }
}
