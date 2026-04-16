using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project_music.DTOs.History;
using project_music.Services.History;
using System.Security.Claims;

namespace project_music.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] 
    public class HistoryController : ControllerBase
    {
        private readonly IHistoryService _historyService;

        public HistoryController(IHistoryService historyService)
        {
            _historyService = historyService;
        }

        [HttpPost("record")]
        public async Task<IActionResult> RecordPlay([FromBody] RecordPlayRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                await _historyService.RecordPlayAsync(userId!, request);
                return Ok(new { message = "Đã lưu lịch sử nghe nhạc." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("recently-played")]
        public async Task<IActionResult> GetRecentlyPlayed()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _historyService.GetRecentlyPlayedAsync(userId!);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}