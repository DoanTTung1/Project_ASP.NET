using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using project_music.DTOs.Auth;
using project_music.Services.Auth;
using System.Security.Claims;
using project_music.Models;
namespace project_music.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        private readonly IAuthService _authService;
        private readonly MusicDbContext _context;

        public AuthController(IAuthService authService, MusicDbContext context)
        {
            _authService = authService;
            _context = context;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var result = await _authService.RegisterAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _authService.LoginAsync(request);
                return Ok(result); // 200 OK kèm theo Token
            }
            catch (Exception ex)
            {
                // Sai tài khoản/mật khẩu thì trả về 401
                return Unauthorized(new { message = ex.Message });
            }
        }

        // --- 2. API ĐĂNG NHẬP BẰNG ZALO ---
        [HttpPost("zalo-login")]
        public async Task<IActionResult> ZaloLogin([FromBody] ZaloLoginRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _authService.LoginWithZaloAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        //--- 3. API LÀM MỚI TOKEN ---
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            try
            {
                var result = await _authService.RefreshTokenAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email))
                    return BadRequest(new { message = "Vui lòng nhập Email." });

                await _authService.ForgotPasswordAsync(request.Email);

                return Ok(new { message = "Mật khẩu mới đã được gửi. Vui lòng kiểm tra hộp thư đến (hoặc thư rác) của bạn!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        [HttpPut("change-password")]
        [Authorize] // Bắt buộc phải đăng nhập
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var user = await _context.Users.FindAsync(userId);

                if (user == null || user.IsDeleted == true)
                    return Unauthorized(new { message = "Tài khoản không tồn tại hoặc đã bị khóa." });

                // Kiểm tra mật khẩu cũ
                bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);
                if (!isPasswordValid)
                    return BadRequest(new { message = "Mật khẩu hiện tại không chính xác." });

                // Kiểm tra mật khẩu mới không được trùng mật khẩu cũ (tùy chọn)
                if (request.CurrentPassword == request.NewPassword)
                    return BadRequest(new { message = "Mật khẩu mới phải khác mật khẩu hiện tại." });

                // Băm mật khẩu mới và lưu
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đổi mật khẩu thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
