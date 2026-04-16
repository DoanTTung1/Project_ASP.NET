using project_music.DTOs.Playlists;

namespace project_music.Services.Playlists
{
    public interface IPlaylistService
    {
        Task<PlaylistResponse> CreatePlaylistAsync(string userId, CreatePlaylistRequest request);

        Task<List<PlaylistResponse>> GetMyPlaylistsAsync(string userId);

        Task<bool> AddSongToPlaylistAsync(string userId,string playlistId,string songId);


        // Lấy chi tiết Playlist (Cần truyền currentUserId để check quyền xem Playlist Private)
        Task<PlaylistDetailResponse> GetPlaylistByIdAsync(string playlistId, string? currentUserId);

        // Xóa bài hát khỏi Playlist
        Task<bool> RemoveSongFromPlaylistAsync(string userId, string playlistId, string songId);

        Task<bool> UpdatePlaylistAsync(string userId, string playlistId, string name, string description);

        Task<bool> DeletePlaylistAsync(string userId, string playlistId);
    }
}