using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project_music.DTOs.Downloads;
using project_music.Services.Downloads;
using System.Security.Claims;

namespace project_music.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Chắc chắn phải đăng nhập
    public class DownloadsController : ControllerBase
    {
        private readonly IDownloadService _downloadService;

        public DownloadsController(IDownloadService downloadService)
        {
            _downloadService = downloadService;
        }

        [HttpPost("request-download")]
        public async Task<IActionResult> RequestDownload([FromBody] DownloadRequest request)
        {
            // Kiểm tra xem dữ liệu gửi lên có bị thiếu ID bài hát không
            if (!ModelState.IsValid)
            {
                return BadRequest(new { message = "Dữ liệu yêu cầu tải xuống không hợp lệ." });
            }

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _downloadService.ProcessDownloadAsync(userId!, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Nếu lỗi do cố tình tải nhạc VIP -> Báo 403 Forbidden
                if (ex.Message.Contains("Premium") || ex.Message.Contains("Độc Quyền"))
                {
                    return StatusCode(403, new { message = ex.Message });
                }

                // Các lỗi khác -> Báo 400 kèm message chuẩn
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}