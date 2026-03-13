using project_music.DTOs.Songs;

namespace project_music.Services.Songs
{
    public interface ISongService
    {
        Task<List<SongResponse>> GetAllAsync();
        Task<SongResponse?> GetByIdAsync(string id);
        Task<SongResponse> CreateAsync(CreateSongRequest request);
        Task<bool> DeleteAsync(string id); // Sẽ dùng Xóa mềm
    }
}