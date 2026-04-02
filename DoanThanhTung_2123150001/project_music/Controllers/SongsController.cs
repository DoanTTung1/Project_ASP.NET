using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using project_music.DTOs.Songs;
using project_music.Services.Songs;
using System.Security.Claims;
namespace project_music.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class SongsController : ControllerBase
    {
        private readonly ISongService _songService;

        public SongsController(ISongService songService)
        {
            _songService = songService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll() => Ok(await _songService.GetAllAsync());

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _songService.GetByIdAsync(id);
            return result == null ? NotFound(new { message = "Không tìm thấy bài hát." }) : Ok(result);
        }

        [HttpPost]
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
                return BadRequest(new {message =ex.Message});
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> Delete(string id)
        {
            var deleted = await _songService.DeleteAsync(id);
            return deleted ? Ok(new { message = "Xóa bài hát thành công (Xóa mềm)." }) : NotFound(new { message = "Không tìm thấy." });
        }

        // --- API THẢ TIM BÀI HÁT ---
        [HttpPost("{id}/favorite")]
        [Authorize] 
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


        // --- API XEM DANH SÁCH YÊU THÍCH ---
        [HttpGet("my-favorites")]
        [Authorize]
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