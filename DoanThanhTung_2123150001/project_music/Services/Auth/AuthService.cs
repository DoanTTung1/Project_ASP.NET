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
using SendGrid;
using SendGrid.Helpers.Mail;

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

        // --- HÀM TẠO REFRESH TOKEN ---
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
                IsDeleted = false
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return new RegisterResponse
            {
                UserId = newUser.UserId,
                Email = newUser.Email,
                Username = newUser.Username,
                PhoneNumber = newUser.PhoneNumber,
                Role = newUser.Role,
                IsPremium = newUser.IsPremium ?? false // 👉 Truyền biến
            };
        }

        public async Task<AuthResponse> LoginAsync(LoginRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() && u.IsDeleted == false);
            if (user == null) throw new Exception("Tài khoản hoặc mật khẩu không chính xác.");

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);
            if (!isPasswordValid) throw new Exception("Tài khoản hoặc mật khẩu không chính xác.");

            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30);
            await _context.SaveChangesAsync();

            return CreateAuthResponse(user, refreshToken);
        }
        // --- HÀM TẠO MẬT KHẨU NGẪU NHIÊN 8 KÝ TỰ ---
        private string GenerateRandomPassword(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";
            var random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        // --- HÀM XỬ LÝ QUÊN MẬT KHẨU ---
        public async Task<bool> ForgotPasswordAsync(string email)
        {
            // 1. Kiểm tra xem Email có tồn tại trong hệ thống không
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower() && u.IsDeleted == false);
            if (user == null)
            {
                throw new Exception("Không tìm thấy tài khoản nào đăng ký bằng Email này.");
            }

            // 2. Tạo mật khẩu mới và mã hóa nó
            string newPassword = GenerateRandomPassword(8);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            await _context.SaveChangesAsync(); // Lưu ngay vào Database

            // 3. Gửi Email chứa mật khẩu mới qua SendGrid
            var apiKey = "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"; // Key của Boss (Nhớ đổi sau nhé)
            var client = new SendGridClient(apiKey);

            var from = new EmailAddress("photonics.xray02558@gmail.com", "Âm Vang Music");
            var subject = "Yêu cầu cấp lại mật khẩu - Âm Vang Music";
            var to = new EmailAddress(user.Email, user.Username);

            var plainTextContent = $"Chào {user.Username}, Mật khẩu mới của bạn là: {newPassword}. Vui lòng đăng nhập và đổi mật khẩu ngay.";
            var htmlContent = $@"
                <div style='font-family: Arial, sans-serif; background-color: #050505; color: #ffffff; padding: 30px; border-radius: 10px;'>
                    <h2 style='color: #dc2626;'>Âm Vang Music</h2>
                    <p>Chào <strong>{user.Username}</strong>,</p>
                    <p>Hệ thống đã nhận được yêu cầu cấp lại mật khẩu của bạn. Đây là mật khẩu mới để bạn đăng nhập:</p>
                    <div style='background-color: #1f2937; padding: 15px; font-size: 24px; font-weight: bold; letter-spacing: 2px; text-align: center; border-radius: 5px; color: #fbbf24; margin: 20px 0;'>
                        {newPassword}
                    </div>
                    <p style='color: #9ca3af; font-size: 12px;'><i>Lưu ý: Vì lý do bảo mật, vui lòng đổi mật khẩu ngay sau khi đăng nhập thành công.</i></p>
                </div>";

            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg);

            if (!response.IsSuccessStatusCode)
            {
                // Nếu gửi mail xịt (do Key lỗi, rớt mạng...), báo lỗi cho Frontend
                throw new Exception("Lỗi hệ thống khi gửi Email. Vui lòng thử lại sau.");
            }

            return true;
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

            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30);
            await _context.SaveChangesAsync();

            return CreateAuthResponse(user, refreshToken);
        }

        public async Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.RefreshToken == request.RefreshToken && u.IsDeleted == false);

            if (user == null || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new Exception("Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
            }

            var newRefreshToken = GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(30);
            await _context.SaveChangesAsync();

            return CreateAuthResponse(user, newRefreshToken);
        }

        // --- HÀM GỘP: CHUYÊN ĐÓNG GÓI TOKEN ---
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
                RefreshToken = refreshToken,
                User = new RegisterResponse
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber ?? "",
                    Role = user.Role ?? "User",
                    IsPremium = user.IsPremium ?? false 
                }
            };
        }
    }
}