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
    }
}