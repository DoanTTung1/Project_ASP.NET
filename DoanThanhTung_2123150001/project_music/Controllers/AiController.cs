using Microsoft.AspNetCore.Mvc;
using project_music.Services.AI;

namespace project_music.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AiController : ControllerBase
    {
        private readonly IAiService _aiService;

        public AiController(IAiService aiService)
        {
            _aiService = aiService;
        }

        [HttpPost("chat")]
        public async Task<IActionResult> Chat([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest(new { message = "Boss phải nhập gì đó để hỏi AI chứ!" });

            try
            {
                // Gọi sang Service để lấy câu trả lời từ Gemini
                var reply = await _aiService.GetChatbotResponseAsync(request.Message);
                return Ok(new { reply = reply });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }

    // Class hứng dữ liệu từ React gửi lên
    public class ChatRequest
    {
        public string Message { get; set; } = null!;
    }
}