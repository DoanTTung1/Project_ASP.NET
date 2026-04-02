using System.ComponentModel.DataAnnotations;

namespace project_music.DTOs.Auth
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng hợp lệ.")]
        [MaxLength(100, ErrorMessage = "Email không được vượt quá 100 kí tự")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "mat khau khong duoc de trong")]
        [MinLength(8, ErrorMessage = "Do dai toi thieu la 8 ki tu")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Mật khẩu phải chứa ít nhất 1 chữ hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt (@$!%*?&)")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Username không được để trống.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Username phải có độ dài từ 2 đến 50 kí tự.")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Phone number không được để trống.")]
        [RegularExpression(@"^(0[3|5|7|8|9])+([0-9]{8})$",
            ErrorMessage = "Số điện thoại không hợp lệ (Phải là số Việt Nam gồm 10 chữ số)")]
        public string phoneNumber { get; set; } = null!; // Giữ nguyên chữ p thường của bạn
    }

    public class RegisterResponse
    {
        public string UserId { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Role { get; set; } = null!;
    }

    // 1. THÊM CLASS NÀY CHO API LOGIN BÌNH THƯỜNG
    public class LoginRequest
    {
        [Required(ErrorMessage = "Vui lòng nhập Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập Mật khẩu")]
        public string Password { get; set; } = null!;
    }

    public class ZaloLoginRequest
    {
        [Required(ErrorMessage = "Thiếu Zalo Access Token")]
        public string AccessToken { get; set; } = null!;
    }

    public class AuthResponse
    {
        public string Token { get; set; } = null!;

        public string RefreshToken { get; set; } = null!;

        // 2. SỬA LẠI CHỖ NÀY: Kiểu dữ liệu là class RegisterResponse, đặt tên biến là User
        public RegisterResponse User { get; set; } = null!;
    }

    public class RefreshTokenRequest
    {
               [Required(ErrorMessage = "Refresh token không được để trống.")]
        public string RefreshToken { get; set; } = null!;

        [Required (ErrorMessage = "Access token không được để trống.")]
        public string Token { get; set; } = null!;

    }
}