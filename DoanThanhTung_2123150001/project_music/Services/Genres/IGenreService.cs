using project_music.DTOs.Genres;

namespace project_music.Services.Genres
{
    public interface IGenreService
    {
        Task<List<GenreResponse>> GetAllAsync();
        Task<GenreResponse?> GetByIdAsync(int id);
        Task<GenreResponse> CreateAsync(CreateGenreRequest request);
        Task<GenreResponse?> UpdateAsync(int id, UpdateGenreRequest request);
        Task<bool> DeleteAsync(int id);
    }
}