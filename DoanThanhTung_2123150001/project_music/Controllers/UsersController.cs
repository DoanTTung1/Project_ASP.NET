using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using project_music.Models;
using System.Security.Claims;

namespace project_music.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly MusicDbContext _context;

        public UsersController(MusicDbContext context)
        {
            _context = context;
        }

        // =====================================================================
        // PHẦN 1: API CHO NGƯỜI DÙNG CÁ NHÂN (USER / ADMIN ĐỀU DÙNG ĐƯỢC)
        // =====================================================================

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return Unauthorized(new { message = "Phiên đăng nhập không hợp lệ." });

                var user = await _context.Users.FindAsync(userId);
                if (user == null || user.IsDeleted == true)
                    return NotFound(new { message = "Không tìm thấy thông tin tài khoản." });

                return Ok(new
                {
                    userId = user.UserId,
                    email = user.Email,
                    username = user.Username,
                    phoneNumber = user.PhoneNumber,
                    role = user.Role,
                    isPremium = user.IsPremium,
                    premiumExpiryDate = user.PremiumExpiryDate
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId == null) return Unauthorized(new { message = "Phiên đăng nhập không hợp lệ." });

                var user = await _context.Users.FindAsync(userId);
                if (user == null || user.IsDeleted == true)
                    return NotFound(new { message = "Không tìm thấy thông tin tài khoản." });

                if (string.IsNullOrWhiteSpace(request.Username))
                    return BadRequest(new { message = "Tên hiển thị không được để trống." });

                user.Username = request.Username;
                user.PhoneNumber = request.PhoneNumber;

                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật hồ sơ thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =====================================================================
        // PHẦN 2: API CHO ADMIN QUẢN LÝ (CHỈ ADMIN MỚI ĐƯỢC GỌI)
        // =====================================================================

        // 2.1. Lấy danh sách toàn bộ người dùng (Có lọc VIP/Thường, Admin/User)
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllUsers([FromQuery] string? role, [FromQuery] bool? isPremium)
        {
            try
            {
                var query = _context.Users.Where(u => u.IsDeleted == false).AsQueryable();

                // Lọc theo Role (Admin / User)
                if (!string.IsNullOrEmpty(role))
                {
                    query = query.Where(u => u.Role == role);
                }

                // Lọc theo VIP (true / false)
                if (isPremium.HasValue)
                {
                    query = query.Where(u => u.IsPremium == isPremium.Value);
                }

                var users = await query.Select(u => new
                {
                    u.UserId,
                    u.Email,
                    u.Username,
                    u.PhoneNumber,
                    u.Role,
                    u.IsPremium,
                    u.PremiumExpiryDate,
                    u.CreatedAt
                }).OrderByDescending(u => u.CreatedAt).ToListAsync();

                return Ok(users);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 2.2. Xem chi tiết 1 người dùng bằng ID
        [Authorize(Roles = "Admin")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id && u.IsDeleted == false);
            if (user == null) return NotFound(new { message = "Không tìm thấy người dùng." });

            return Ok(new
            {
                user.UserId,
                user.Email,
                user.Username,
                user.PhoneNumber,
                user.Role,
                user.IsPremium,
                user.PremiumExpiryDate,
                user.CreatedAt
            });
        }

        // 2.3. Thêm mới một người dùng (Admin cấp acc)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] AdminCreateUserRequest request)
        {
            try
            {
                // Kiểm tra email trùng
                var exists = await _context.Users.AnyAsync(u => u.Email == request.Email && u.IsDeleted == false);
                if (exists) return BadRequest(new { message = "Email này đã được sử dụng." });

                // Mã hóa mật khẩu (Nên dùng thư viện BCrypt thực tế, ở đây dùng tạm mã hóa cứng nếu Boss chưa có AuthService dùng chung)
                var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

                var newUser = new User
                {
                    UserId = Guid.NewGuid().ToString(),
                    Email = request.Email,
                    PasswordHash = passwordHash,
                    Username = request.Username,
                    PhoneNumber = request.PhoneNumber,
                    Role = string.IsNullOrEmpty(request.Role) ? "User" : request.Role, // Mặc định là User
                    IsPremium = request.IsPremium,
                    PremiumExpiryDate = request.IsPremium ? DateTime.UtcNow.AddDays(30) : null, // Mặc định cho 30 ngày VIP nếu set true
                    CreatedAt = DateTime.UtcNow,
                    IsDeleted = false
                };

                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Tạo người dùng thành công!", userId = newUser.UserId });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 2.4. Sửa thông tin người dùng (Đổi quyền, Nâng VIP, Sửa tên...)
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUserByAdmin(string id, [FromBody] AdminUpdateUserRequest request)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id && u.IsDeleted == false);
                if (user == null) return NotFound(new { message = "Không tìm thấy người dùng." });

                user.Username = request.Username;
                user.PhoneNumber = request.PhoneNumber;

                // Admin có quyền can thiệp Role và trạng thái VIP
                user.Role = request.Role;
                user.IsPremium = request.IsPremium;

                // Xử lý hạn VIP
                if (request.IsPremium && user.PremiumExpiryDate == null)
                {
                    user.PremiumExpiryDate = DateTime.UtcNow.AddMonths(1); // Mới lên VIP thì cho 1 tháng
                }
                else if (!request.IsPremium)
                {
                    user.PremiumExpiryDate = null; // Rớt đài xuống thường thì xóa hạn VIP
                }

                await _context.SaveChangesAsync();

                return Ok(new { message = "Cập nhật tài khoản thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 2.5. Xóa người dùng (Soft Delete - Cắt chức năng nhưng giữ data)
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            try
            {
                // Không cho phép Admin tự xóa chính mình
                var currentAdminId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (id == currentAdminId) return BadRequest(new { message = "Boss không thể tự đưa mình lên máy chém được!" });

                var user = await _context.Users.FirstOrDefaultAsync(u => u.UserId == id && u.IsDeleted == false);
                if (user == null) return NotFound(new { message = "Không tìm thấy người dùng." });

                // Soft Delete: Chuyển cờ IsDeleted thành true
                user.IsDeleted = true;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Đã xóa người dùng khỏi hệ thống." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    // =====================================================================
    // CÁC LỚP DTO (DATA TRANSFER OBJECT) ĐỂ HỨNG DỮ LIỆU TỪ REACT GỬI LÊN
    // =====================================================================

    public class UpdateProfileRequest
    {
        public string Username { get; set; } = null!;
        public string? PhoneNumber { get; set; }
    }

    public class AdminCreateUserRequest
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = "User"; // "Admin" hoặc "User"
        public bool IsPremium { get; set; } = false;
    }

    public class AdminUpdateUserRequest
    {
        public string Username { get; set; } = null!;
        public string? PhoneNumber { get; set; }
        public string Role { get; set; } = null!; // Bắt buộc phải có khi cập nhật
        public bool IsPremium { get; set; }
    }
}