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

        
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePlaylistRequest request)
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



        // --- 4. API LẤY CHI TIẾT PLAYLIST KÈM BÀI HÁT ---
        [HttpGet("{id}")]
        [AllowAnonymous] // Bất kỳ ai cũng có thể gọi API này (để xem Playlist public)
        public async Task<IActionResult> GetPlaylistById(string id)
        {
            try
            {
                // Lấy UserId nếu họ có truyền Token (có thể null nếu khách vãng lai)
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                var result = await _playlistService.GetPlaylistByIdAsync(id, currentUserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }


        // --- API SỬA THÔNG TIN PLAYLIST ---
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdatePlaylist(string id, [FromBody] CreatePlaylistRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId)) return Unauthorized();

                await _playlistService.UpdatePlaylistAsync(userId, id, request.Name, request.Description);
                return Ok(new { message = "Đã cập nhật Playlist thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // --- API XÓA PLAYLIST ---
        [HttpDelete("{id}")]
        [Authorize]
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

        // --- 5. API XÓA BÀI HÁT KHỎI PLAYLIST ---
        [HttpDelete("{id}/songs/{songId}")]
        // Mặc định API này bị dính [Authorize] của class nên bắt buộc phải đăng nhập
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