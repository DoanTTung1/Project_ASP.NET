using project_music.DTOs.Songs;

namespace project_music.Services.Songs
{
    public interface ISongService
    {
        Task<List<SongResponse>> GetAllAsync();
        Task<SongResponse?> GetByIdAsync(string id);
        Task<SongResponse> CreateAsync(CreateSongRequest request);
        Task<bool> DeleteAsync(string id);

        Task<bool> ToggleFavoriteAsync(string userId, string songId);

        Task<List<SongResponse>> GetMyFavoriteSongsAsync(string userId);

        Task<SongResponse> UpdateAsync(string id, UpdateSongRequest request);
    }
}