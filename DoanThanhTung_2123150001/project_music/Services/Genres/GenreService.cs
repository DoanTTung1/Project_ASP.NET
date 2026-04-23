using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using project_music.DTOs.Genres;
using project_music.DTOs.Songs;
using project_music.Models;

namespace project_music.Services.Genres
{
    public class GenreService : IGenreService
    {
        private readonly MusicDbContext _context;
        private readonly IWebHostEnvironment _env; // 👉 MỚI: Dùng để lấy đường dẫn lưu file

        public GenreService(MusicDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // 👉 HÀM BỔ TRỢ: Lưu file vật lý vào thư mục wwwroot
        private async Task<string?> SaveFileAsync(IFormFile? file)
        {
            if (file == null || file.Length == 0) return null;

            var folderPath = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "genres");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var fileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/genres/{fileName}"; // Trả về đường dẫn tương đối
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
            return await _context.Genres
                .Where(g => g.GenreId == id)
                .Select(g => new GenreResponse
                {
                    GenreId = g.GenreId,
                    Name = g.Name,
                    ImageUrl = g.ImageUrl,
                    // 👉 LẤY RA DANH SÁCH BÀI HÁT CỦA THỂ LOẠI NÀY
                    Songs = g.Songs.Where(s => s.IsDeleted == false).Select(s => new SongResponse
                    {
                        SongId = s.SongId,
                        Title = s.Title,
                        DurationSeconds = s.DurationSeconds,
                        CoverUrl = s.CoverUrl,
                        IsVip = s.IsVip,
                        FileUrl = s.AudioFiles.Select(a => a.FileUrl).FirstOrDefault(),
                        Artists = s.ArtistSongs.Select(ast => new SongArtistResponse
                        {
                            ArtistId = ast.ArtistId,
                            Name = ast.Artist.Name
                        }).ToList()
                    }).ToList()
                }).FirstOrDefaultAsync();
        }

        public async Task<GenreResponse> CreateAsync(CreateGenreRequest request)
        {
            var exists = await _context.Genres.AnyAsync(g => g.Name.ToLower() == request.Name.ToLower());
            if (exists) throw new Exception($"Thể loại '{request.Name}' đã tồn tại.");

            // 👉 XỬ LÝ LƯU FILE
            string? uploadedImageUrl = await SaveFileAsync(request.ImageFile);

            var newGenre = new Genre
            {
                Name = request.Name,
                ImageUrl = uploadedImageUrl
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

            // 👉 CHỈ CẬP NHẬT ẢNH NẾU BOSS CÓ CHỌN FILE MỚI
            if (request.ImageFile != null)
            {
                var uploadedImageUrl = await SaveFileAsync(request.ImageFile);
                if (uploadedImageUrl != null)
                {
                    genre.ImageUrl = uploadedImageUrl;
                }
            }

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