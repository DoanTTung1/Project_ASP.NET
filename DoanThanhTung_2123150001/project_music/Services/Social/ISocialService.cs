using project_music.DTOs.Social;

namespace project_music.Services.Social
{
    public interface ISocialService
    {
        // Hàm Follow / Unfollow ca sĩ
        Task<bool> ToggleFollowArtistAsync(string userId, string artistId);

        // Hàm lấy danh sách ca sĩ đang theo dõi
        Task<List<FollowedArtistResponse>> GetFollowedArtistsAsync(string userId);
    }
}