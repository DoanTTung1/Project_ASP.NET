using Microsoft.EntityFrameworkCore;
using project_music.DTOs.Artists;
using project_music.DTOs.Songs;
using project_music.Models;
using Microsoft.AspNetCore.Http;

namespace project_music.Services.Artists
{
    public class ArtistService : IArtistService
    {
        private readonly MusicDbContext _context;

        public ArtistService(MusicDbContext context)
        {
            _context = context;
        }

        public async Task<List<ArtistResponse>> GetAllArtistsAsync()
        {
            return await _context.Artists
                .Where(a => a.IsDeleted == false) // Lọc bỏ nghệ sĩ đã xóa
                .Select(a => new ArtistResponse
                {
                    ArtistId = a.ArtistId,
                    Name = a.Name,
                    Bio = a.Bio,
                    AvatarUrl = a.AvatarUrl,
                    CoverUrl = a.CoverUrl
                }).ToListAsync();
        }

        public async Task<ArtistResponse?> GetArtistByIdAsync(string artistId)
        {
            var artist = await _context.Artists
                .Where(a => a.ArtistId == artistId && a.IsDeleted == false)
                .Select(a => new ArtistResponse
                {
                    ArtistId = a.ArtistId,
                    Name = a.Name,
                    Bio = a.Bio,
                    AvatarUrl = a.AvatarUrl,
                    CoverUrl = a.CoverUrl,

                    Songs = a.ArtistSongs
                        .Where(ast => ast.Song.IsDeleted == false)
                        .Select(ast => new SongResponse
                        {
                            SongId = ast.Song.SongId,
                            Title = ast.Song.Title,
                            DurationSeconds = ast.Song.DurationSeconds,
                            CoverUrl = ast.Song.CoverUrl ?? (ast.Song.Album != null ? ast.Song.Album.CoverUrl : null),
                            IsVip = ast.Song.IsVip,
                            FileUrl = _context.AudioFiles.Where(af => af.SongId == ast.Song.SongId).Select(af => af.FileUrl).FirstOrDefault(),
                            Artists = ast.Song.ArtistSongs.Select(x => new SongArtistResponse
                            {
                                ArtistId = x.ArtistId,
                                Name = x.Artist.Name
                            }).ToList()
                        }).ToList()
                }).FirstOrDefaultAsync();

            return artist;
        }

        public async Task<ArtistResponse> CreateArtistAsync(CreateArtistRequest request)
        {
            var exists = await _context.Artists.AnyAsync(a => a.Name.ToLower() == request.Name.ToLower() && a.IsDeleted == false);
            if (exists)
            {
                throw new Exception($"Nghệ sĩ mang tên '{request.Name}' đã tồn tại trong hệ thống.");
            }

            var newArtist = new Artist
            {
                ArtistId = Guid.NewGuid().ToString(),
                Name = request.Name.Trim(),
                Bio = request.Bio?.Trim(),

                // 👉 LƯU FILE VÀ LẤY ĐƯỜNG DẪN ẢO
                AvatarUrl = await SaveImageAsync(request.AvatarFile),
                CoverUrl = await SaveImageAsync(request.CoverFile),
                IsDeleted = false
            };

            _context.Artists.Add(newArtist);
            await _context.SaveChangesAsync();

            return new ArtistResponse
            {
                ArtistId = newArtist.ArtistId,
                Name = newArtist.Name,
                Bio = newArtist.Bio,
                AvatarUrl = newArtist.AvatarUrl,
                CoverUrl = newArtist.CoverUrl
            };
        }


        public async Task<ArtistResponse> UpdateArtistAsync(string id, CreateArtistRequest request)
        {
            var artist = await _context.Artists.FirstOrDefaultAsync(a => a.ArtistId == id && a.IsDeleted == false);
            if (artist == null)
            {
                throw new Exception("Không tìm thấy nghệ sĩ để cập nhật.");
            }

            // Kiểm tra xem tên mới có bị trùng với người khác không
            if (!string.Equals(artist.Name, request.Name, StringComparison.OrdinalIgnoreCase))
            {
                var exists = await _context.Artists.AnyAsync(a => a.Name.ToLower() == request.Name.ToLower() && a.IsDeleted == false);
                if (exists) throw new Exception($"Nghệ sĩ mang tên '{request.Name}' đã tồn tại.");
            }

            // Cập nhật thông tin cơ bản
            artist.Name = request.Name.Trim();
            artist.Bio = request.Bio?.Trim();

            // 👉 NẾU FRONTEND CÓ GỬI FILE LÊN THÌ LƯU ĐÈ VÀO DB, KHÔNG THÌ GIỮ NGUYÊN ẢNH CŨ
            if (request.AvatarFile != null)
            {
                artist.AvatarUrl = await SaveImageAsync(request.AvatarFile);
            }

            if (request.CoverFile != null)
            {
                artist.CoverUrl = await SaveImageAsync(request.CoverFile);
            }

            await _context.SaveChangesAsync();

            return new ArtistResponse
            {
                ArtistId = artist.ArtistId,
                Name = artist.Name,
                Bio = artist.Bio,
                AvatarUrl = artist.AvatarUrl,
                CoverUrl = artist.CoverUrl
            };
        }

        public async Task<bool> DeleteArtistAsync(string id)
        {
            var artist = await _context.Artists.FirstOrDefaultAsync(a => a.ArtistId == id && a.IsDeleted == false);
            if (artist == null)
            {
                throw new Exception("Không tìm thấy nghệ sĩ.");
            }

            // Xóa mềm (Soft Delete)
            artist.IsDeleted = true;
            await _context.SaveChangesAsync();
            return true;
        }

        private async Task<string?> SaveImageAsync(IFormFile file)
        {
            if (file == null || file.Length == 0) return null;

            var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "covers");
            if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return "/covers/" + fileName;
        }
    }
}