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
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _downloadService.ProcessDownloadAsync(userId!, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Nếu báo lỗi (Ví dụ: Không phải VIP) thì ném lỗi 403 Forbidden cho Front-end biết
                return StatusCode(403, new { message = ex.Message });
            }
        }
    }
}