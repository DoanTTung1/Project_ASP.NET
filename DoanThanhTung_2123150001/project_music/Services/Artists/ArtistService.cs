using Microsoft.EntityFrameworkCore;
using project_music.DTOs.Artists;
using project_music.Models;

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
            var artist = await _context.Artists.FindAsync(artistId);
            if (artist == null) return null;

            return new ArtistResponse
            {
                ArtistId = artist.ArtistId,
                Name = artist.Name,
                Bio = artist.Bio,
                AvatarUrl = artist.AvatarUrl,
                CoverUrl = artist.CoverUrl
            };
        }

        public async Task<ArtistResponse> CreateArtistAsync(CreateArtistRequest request)
        {
            // Kiểm tra nghiệp vụ chặt chẽ: Không cho phép trùng tên nghệ sĩ
            var exists = await _context.Artists.AnyAsync(a => a.Name.ToLower() == request.Name.ToLower());
            if (exists)
            {
                throw new Exception($"Nghệ sĩ mang tên '{request.Name}' đã tồn tại trong hệ thống.");
            }

            // Map DTO sang Model thực tế của Database
            var newArtist = new Artist
            {
                ArtistId = Guid.NewGuid().ToString(), 
                Name = request.Name,
                Bio = request.Bio,
                AvatarUrl = request.AvatarUrl,
                CoverUrl = request.CoverUrl
            };

            // Lưu vào DB
            _context.Artists.Add(newArtist);
            await _context.SaveChangesAsync();

            // Trả về kết quả sau khi tạo
            return new ArtistResponse
            {
                ArtistId = newArtist.ArtistId,
                Name = newArtist.Name,
                Bio = newArtist.Bio,
                AvatarUrl = newArtist.AvatarUrl,
                CoverUrl = newArtist.CoverUrl
            };
        }
    }
}