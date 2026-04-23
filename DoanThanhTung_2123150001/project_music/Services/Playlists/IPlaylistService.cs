using project_music.DTOs.Playlists;

namespace project_music.Services.Playlists
{
    public interface IPlaylistService
    {
        Task<PlaylistResponse> CreatePlaylistAsync(string userId, CreatePlaylistRequest request);
        Task<List<PlaylistResponse>> GetMyPlaylistsAsync(string userId);
        Task<bool> AddSongToPlaylistAsync(string userId, string playlistId, string songId);
        // 👉 CẬP NHẬT THAM SỐ TẠI ĐÂY
        Task<bool> UpdatePlaylistAsync(string userId, string playlistId, CreatePlaylistRequest request);
        Task<bool> DeletePlaylistAsync(string userId, string playlistId);
        Task<PlaylistDetailResponse> GetPlaylistByIdAsync(string playlistId, string? currentUserId);
        Task<bool> RemoveSongFromPlaylistAsync(string userId, string playlistId, string songId);
        Task<List<PlaylistResponse>> GetAllPlaylistsAsync();
    }
}