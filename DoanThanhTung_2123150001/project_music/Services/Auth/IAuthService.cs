using project_music.DTOs.Auth;

namespace project_music.Services.Auth
{
    public interface IAuthService
    {
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);

        Task<AuthResponse> LoginAsync(LoginRequest request);

        Task<AuthResponse> LoginWithZaloAsync(ZaloLoginRequest request);

        Task<AuthResponse> RefreshTokenAsync(RefreshTokenRequest request);
    }
}