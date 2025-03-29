using Microsoft.AspNetCore.Mvc;
using WordHelper.DTOs;
using WordHelper.Services;

namespace WordHelper.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WordController : ControllerBase
    {
        private readonly IWordService _wordService;

        public WordController(IWordService wordService)
        {
            _wordService = wordService;
        }

        [HttpPost("info")]
        public async Task<IActionResult> GetWordInfo([FromBody] WordRequestDto request)
        {
            var result = await _wordService.GetWordInfoAsync(request.Word);
            return Ok(result);
        }
    }
}
