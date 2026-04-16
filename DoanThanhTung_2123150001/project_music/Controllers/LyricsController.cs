using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project_music.Services.Lyrics;

namespace project_music.Controllers
{
    // Cực kỳ tinh tế: API sẽ có dạng /api/Songs/{songId}/lyrics
    [Route("api/Songs")]
    [ApiController]
    public class LyricsController : ControllerBase
    {
        private readonly ILyricService _lyricService;

        public LyricsController(ILyricService lyricService)
        {
            _lyricService = lyricService;
        }

        [HttpGet("{songId}/lyrics")]
        [AllowAnonymous] // Lời bài hát ai cũng xem được, không cần đăng nhập
        public async Task<IActionResult> GetLyrics(string songId)
        {
            try
            {
                var result = await _lyricService.GetLyricsBySongIdAsync(songId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}