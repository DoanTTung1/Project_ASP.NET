using Microsoft.AspNetCore.Mvc;
using project_music.DTOs.AudioFiles;
using project_music.Services.AudioFiles;
using Microsoft.AspNetCore.Authorization;
namespace project_music.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AudioFilesController : ControllerBase
    {
        private readonly IAudioFileService _audioFileService;

        public AudioFilesController(IAudioFileService audioFileService)
        {
            _audioFileService = audioFileService;
        }

        [HttpPost("upload")]
        // Chú ý: Dùng [FromForm] thay vì [FromBody] để nhận file
        public async Task<IActionResult> Upload([FromForm] UploadAudioRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _audioFileService.UploadAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}