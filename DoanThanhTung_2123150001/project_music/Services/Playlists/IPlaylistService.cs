using project_music.DTOs.Playlists;

namespace project_music.Services.Playlists
{
    public interface IPlaylistService
    {
        Task<PlaylistResponse> CreatePlaylistAsync(string userId, CreatePlaylistRequest request);

        Task<List<PlaylistResponse>> GetMyPlaylistsAsync(string userId);

        Task<bool> AddSongToPlaylistAsync(string userId,string playlistId,string songId);
    }
}