using WordHelper.DTOs;

namespace WordHelper.Services
{
    public interface IWordService
    {
        Task<WordResponseDto> GetWordInfoAsync(string word);
    }
}
