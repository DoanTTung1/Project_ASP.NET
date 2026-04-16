using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using project_music.Services.Social;
using System.Security.Claims;

namespace project_music.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Phải đăng nhập mới được Follow
    public class SocialController : ControllerBase
    {
        private readonly ISocialService _socialService;

        public SocialController(ISocialService socialService)
        {
            _socialService = socialService;
        }

        // --- 1. API FOLLOW / UNFOLLOW CA SĨ ---
        [HttpPost("artists/{artistId}/follow")]
        public async Task<IActionResult> ToggleFollowArtist(string artistId)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Bạn chưa đăng nhập." });

                var isFollowed = await _socialService.ToggleFollowArtistAsync(userId, artistId);

                if (isFollowed)
                    return Ok(new { message = "Đã theo dõi ca sĩ thành công! 🌟", isFollowed = true });
                else
                    return Ok(new { message = "Đã bỏ theo dõi ca sĩ. ✖️", isFollowed = false });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // --- 2. API XEM DANH SÁCH CA SĨ ĐANG THEO DÕI ---
        [HttpGet("artists/following")]
        public async Task<IActionResult> GetFollowingArtists()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { message = "Bạn chưa đăng nhập." });

                var result = await _socialService.GetFollowedArtistsAsync(userId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}