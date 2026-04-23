using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project_music.DTOs.Playlists;
using project_music.Services.Playlists;
using System.Security.Claims;

namespace project_music.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlaylistsController : ControllerBase
    {
        private readonly IPlaylistService _playlistService;

        public PlaylistsController(IPlaylistService playlistService)
        {
            _playlistService = playlistService;
        }

        // --- 1. LẤY TẤT CẢ PLAYLIST (DÀNH CHO ADMIN) ---
        [HttpGet]
        [AllowAnonymous] // Tùy Boss, có thể để AllowAnonymous hoặc bắt buộc Authorize
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _playlistService.GetAllPlaylistsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // --- 2. LẤY PLAYLIST CỦA USER ĐANG ĐĂNG NHẬP ---
        [HttpGet("my-playlists")]
        public async Task<IActionResult> GetMyPlaylists()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Bạn chưa đăng nhập." });

                var result = await _playlistService.GetMyPlaylistsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // --- 3. LẤY CHI TIẾT PLAYLIST KÈM BÀI HÁT ---
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPlaylistById(string id)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var result = await _playlistService.GetPlaylistByIdAsync(id, currentUserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // --- 4. TẠO MỚI PLAYLIST ---
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreatePlaylistRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Bạn chưa đăng nhập." });

                var result = await _playlistService.CreatePlaylistAsync(userId, request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // --- 5. SỬA THÔNG TIN PLAYLIST ---
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePlaylist(string id, [FromForm] CreatePlaylistRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                await _playlistService.UpdatePlaylistAsync(userId, id, request);
                return Ok(new { message = "Đã cập nhật Playlist thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // --- 6. XÓA PLAYLIST ---
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlaylist(string id)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                await _playlistService.DeletePlaylistAsync(userId, id);
                return Ok(new { message = "Đã xóa Playlist thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // --- 7. THÊM BÀI HÁT VÀO PLAYLIST ---
        [HttpPost("{id}/songs")]
        public async Task<IActionResult> AddSongToPlaylist(string id, [FromBody] AddSongRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Bạn chưa đăng nhập." });

                await _playlistService.AddSongToPlaylistAsync(userId, id, request.SongId);
                return Ok(new { message = "Đã thêm bài hát vào Playlist thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // --- 8. XÓA BÀI HÁT KHỎI PLAYLIST ---
        [HttpDelete("{id}/songs/{songId}")]
        public async Task<IActionResult> RemoveSongFromPlaylist(string id, string songId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Bạn chưa đăng nhập." });

                await _playlistService.RemoveSongFromPlaylistAsync(userId, id, songId);
                return Ok(new { message = "Đã xóa bài hát khỏi Playlist thành công! 🗑️" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}