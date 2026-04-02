using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using project_music.DTOs.Auth;
using project_music.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography; 

namespace project_music.Services.Auth
{
    public class AuthService : IAuthService
    {
        private readonly MusicDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(MusicDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // --- HÀM MỚI: TẠO REFRESH TOKEN NGẪU NHIÊN ---
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            var exits = await _context.Users.AnyAsync(u => u.Email.ToLower() == request.Email.ToLower());
            if (exits)
            {
                throw new Exception("Người dùng đã tồn tại.");
            }

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var newUser = new User
            {
                UserId = Guid.NewGuid().ToString(),
                Email = request.Email,
                PasswordHash = hashedPassword,
                Username = request.Username,
                PhoneNumber = request.phoneNumber,
                IsPremium = false,
                Role = "User",
                IsDeleted = false // Set mặc định chưa xóa
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return new RegisterResponse
            {
                UserId = newUser.UserId,
                Email = newUser.Email,
                Username = newUser.Username,
                PhoneNumber = newUser.PhoneNumber,
                Role = newUser.Role
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            // Thêm điều kiện IsDeleted == false (Không cho nick bị xóa đăng nhập)
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() && u.IsDeleted == false);
            if (user == null) throw new Exception("Tài khoản hoặc mật khẩu không chính xác.");

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid) throw new Exception("Tài khoản hoặc mật khẩu không chính xác.");

            // TẠO REFRESH TOKEN VÀ LƯU VÀO DB
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30); // Cho sống 30 ngày
            await _context.SaveChangesAsync();

            // Gọi hàm đóng gói Token ở dưới cùng
            return CreateAuthResponse(user, refreshToken);
        }

        public async Task<AuthResponse> LoginWithZaloAsync(ZaloLoginRequest request)
        {
            using var client = new HttpClient();
            var zaloUrl = $"https://graph.zalo.me/v2.0/me?access_token={request.AccessToken}&fields=id,name,picture";

            var response = await client.GetAsync(zaloUrl);
            if (!response.IsSuccessStatusCode) throw new Exception("Token Zalo không hợp lệ hoặc đã hết hạn.");

            var content = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            if (root.TryGetProperty("error", out var errorProp) && errorProp.GetInt32() != 0)
            {
                throw new Exception("Lỗi từ máy chủ Zalo: " + root.GetProperty("message").GetString());
            }

            string zaloId = root.GetProperty("id").GetString()!;
            string zaloName = root.GetProperty("name").GetString()!;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.ZaloId == zaloId);

            if (user == null)
            {
                user = new User
                {
                    UserId = Guid.NewGuid().ToString(),
                    ZaloId = zaloId,
                    Username = zaloName,
                    Email = $"zalo_{zaloId}@zalo.local",
                    PhoneNumber = "0999999999",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
                    Role = "User",
                    IsPremium = false,
                    IsDeleted = false
                };

                _context.Users.Add(user);
            }
            else if (user.IsDeleted == true)
            {
                throw new Exception("Tài khoản này đã bị khóa hoặc xóa.");
            }

            // TẠO REFRESH TOKEN VÀ LƯU VÀO DB
            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30);
            await _context.SaveChangesAsync();

            // Gọi hàm đóng gói Token
            return CreateAuthResponse(user, refreshToken);
        }

        // --- HÀM MỚI: XIN CẤP LẠI TOKEN ---
        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            // Tìm User có cái RefreshToken này
            var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken && u.IsDeleted == false);

            // Kiểm tra xem mã có tồn tại và còn hạn không
            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new Exception("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
            }

            // Sinh mã Refresh Token mới để xoay vòng bảo mật
            var newRefreshToken = GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30);
            await _context.SaveChangesAsync();

            return CreateAuthResponse(user, newRefreshToken);
        }

        // --- HÀM GỘP: CHUYÊN ĐÓNG GÓI TOKEN (Giúp code không bị lặp lại) ---
        private AuthResponse CreateAuthResponse(User user, string refreshToken)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]!);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.UserId),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role ?? "User")
                }),
                Expires = DateTime.UtcNow.AddDays(double.Parse(_config["Jwt:ExpireDays"]!)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return new AuthResponse
            {
                Token = tokenHandler.WriteToken(token),
                RefreshToken = refreshToken, // <--- Trả về Refresh Token cho FE
                User = new RegisterResponse
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber ?? "",
                    Role = user.Role ?? "User"
                }
            };
        }
    }
}