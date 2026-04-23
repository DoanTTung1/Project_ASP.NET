using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project_music.DTOs.Songs;
using project_music.Services.AudioFiles;
using project_music.Services.Songs;
using System.Security.Claims;

namespace project_music.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // 👉 ĐÃ XÓA [Authorize(Roles = "Admin")] Ở ĐÂY ĐỂ MỞ CỬA CHO MỌI NGƯỜI
    public class SongsController : ControllerBase
    {
        private readonly ISongService _songService;
        private readonly IAudioFileService _audioFileService;

        public SongsController(ISongService songService, IAudioFileService audioFileService)
        {
            _songService = songService;
            _audioFileService = audioFileService;
        }

        // --- KHU VỰC KHÁCH VÃNG LAI (Không cần đăng nhập) ---

        [HttpGet]
        public async Task<IActionResult> GetAll() => Ok(await _songService.GetAllAsync());

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _songService.GetByIdAsync(id);
            return result == null ? NotFound(new { message = "Không tìm thấy bài hát." }) : Ok(result);
        }

        // --- KHU VỰC ADMIN (Chỉ Admin mới được Thêm/Sửa/Xóa) ---

        [HttpPost]
        [Authorize(Roles = "Admin")] // 👉 ĐEM XUỐNG ĐÂY
        public async Task<IActionResult> Create([FromBody] CreateSongRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var result = await _songService.CreateAsync(request);
                return CreatedAtAction(nameof(GetById), new { id = result.SongId }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")] // 👉 VÀ ĐÂY
        public async Task<IActionResult> Update(string id, [FromBody] UpdateSongRequest request)
        {
            var result = await _songService.UpdateAsync(id, request);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // 👉 VÀ ĐÂY
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await _songService.DeleteAsync(id);
            return deleted ? Ok(new { message = "Xóa bài hát thành công (Xóa mềm)." }) : NotFound(new { message = "Không tìm thấy." });
        }

        [HttpPost("{id}/upload-cover")]
        [Authorize(Roles = "Admin")] // 👉 VÀ ĐÂY
        public async Task<IActionResult> UploadCover(string id, IFormFile file)
        {
            var url = await _audioFileService.SaveCoverAsync(id, file);
            return Ok(new { coverUrl = url });
        }


        // --- KHU VỰC USER THƯỜNG (Chỉ cần đăng nhập) ---

        [HttpPost("{id}/favorite")]
        [Authorize] // 👉 Tài khoản thường (User) cũng được Thả tim
        public async Task<IActionResult> ToggleFavorite(string id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Bạn chưa đăng nhập." });

                var isLiked = await _songService.ToggleFavoriteAsync(userId, id);

                if (isLiked)
                    return Ok(new { message = "Đã thêm vào danh sách yêu thích ❤️", isFavorite = true });
                else
                    return Ok(new { message = "Đã bỏ yêu thích 💔", isFavorite = false });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("my-favorites")]
        [Authorize] // 👉 Tài khoản thường được xem danh sách tim của mình
        public async Task<IActionResult> GetMyFavorites()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Bạn chưa đăng nhập." });

                var result = await _songService.GetMyFavoriteSongsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}