using Microsoft.EntityFrameworkCore;
using project_music.DTOs.Genres;
using project_music.Models;

// Chú ý namespace theo đúng thư mục
namespace project_music.Services.Genres
{
    public class GenreService : IGenreService
    {
        private readonly MusicDbContext _context;

        public GenreService(MusicDbContext context)
        {
            _context = context;
        }

        public async Task<List<GenreResponse>> GetAllAsync()
        {
            return await _context.Genres
                .Select(g => new GenreResponse
                {
                    GenreId = g.GenreId,
                    Name = g.Name,
                    ImageUrl = g.ImageUrl
                }).ToListAsync();
        }

        public async Task<GenreResponse?> GetByIdAsync(int id)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null) return null;

            return new GenreResponse
            {
                GenreId = genre.GenreId,
                Name = genre.Name,
                ImageUrl = genre.ImageUrl
            };
        }

        public async Task<GenreResponse> CreateAsync(CreateGenreRequest request)
        {
            var exists = await _context.Genres.AnyAsync(g => g.Name.ToLower() == request.Name.ToLower());
            if (exists) throw new Exception($"Thể loại '{request.Name}' đã tồn tại.");

            var newGenre = new Genre
            {
                Name = request.Name,
                ImageUrl = request.ImageUrl
            };

            _context.Genres.Add(newGenre);
            await _context.SaveChangesAsync();

            return new GenreResponse
            {
                GenreId = newGenre.GenreId,
                Name = newGenre.Name,
                ImageUrl = newGenre.ImageUrl
            };
        }

        public async Task<GenreResponse?> UpdateAsync(int id, UpdateGenreRequest request)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null) return null;

            var exists = await _context.Genres.AnyAsync(g => g.Name.ToLower() == request.Name.ToLower() && g.GenreId != id);
            if (exists) throw new Exception($"Thể loại '{request.Name}' đã tồn tại.");

            genre.Name = request.Name;
            genre.ImageUrl = request.ImageUrl;

            await _context.SaveChangesAsync();

            return new GenreResponse
            {
                GenreId = genre.GenreId,
                Name = genre.Name,
                ImageUrl = genre.ImageUrl
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre == null) return false;

            _context.Genres.Remove(genre);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}